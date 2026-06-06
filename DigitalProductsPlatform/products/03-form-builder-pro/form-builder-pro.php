<?php
/**
 * Plugin Name: Form Builder Pro
 * Plugin URI: https://creativstudio.gumroad.com/l/form-builder-pro
 * Description: Drag & drop form builder - contact forms, surveys, payment forms, email marketing
 * Version: 2.8.0
 * Author: creativstudio
 * License: GPL v2 or later
 */

if (!defined('ABSPATH')) exit;

class Form_Builder_Pro {
    
    private static $instance = null;
    private $product_permalink = 'form-builder-pro';
    
    public static function instance() {
        if (is_null(self::$instance)) {
            self::$instance = new self();
        }
        return self::$instance;
    }
    
    private function __construct() {
        register_activation_hook(__FILE__, array($this, 'activate'));
        add_action('admin_menu', array($this, 'admin_menu'));
        add_shortcode('digi_form', array($this, 'form_shortcode'));
        add_action('wp_ajax_fbp_save_form', array($this, 'ajax_save_form'));
        add_action('wp_ajax_fbp_submit_form', array($this, 'ajax_submit_form'));
        add_action('admin_init', array($this, 'handle_license_activation'));
        add_action('admin_notices', array($this, 'license_admin_notice'));
    }

    // ============ LICENSE ============
    public function get_license_key() { return get_option('fbp_license_key', ''); }
    public function get_license_status() { return get_option('fbp_license_status', 'invalid'); }
    public function get_license_tier() { return get_option('fbp_license_tier', 'none'); }

    public function has_feature($feature) {
        $tier = strtolower($this->get_license_tier());
        if ($this->get_license_status() !== 'valid') return false;
        $features = array(
            'basic' => array('form_builder', 'email_notifications', 'spam_check', 'basic_fields', 'submissions', 'basic_support'),
            'pro' => array('form_builder', 'email_notifications', 'spam_check', 'basic_fields', 'submissions', 'basic_support', 'file_upload', 'recaptcha', 'webhook', 'csv_export', 'priority_support'),
            'enterprise' => array('form_builder', 'email_notifications', 'spam_check', 'basic_fields', 'submissions', 'basic_support', 'file_upload', 'recaptcha', 'webhook', 'csv_export', 'priority_support', 'payment_integration', 'email_marketing', 'custom_templates', 'white_label', 'dedicated_support'),
        );
        return isset($features[$tier]) && in_array($feature, $features[$tier]);
    }

    public function get_site_count() {
        $t = strtolower($this->get_license_tier());
        if ($t === 'basic') return 1;
        if ($t === 'pro') return 5;
        if ($t === 'enterprise') return -1;
        return 0;
    }

    public function handle_license_activation() {
        if (!isset($_POST['fbp_activate_license']) && !isset($_POST['fbp_deactivate_license'])) return;
        if (!current_user_can('manage_options')) return;
        check_admin_referer('fbp_license_action');
        if (isset($_POST['fbp_deactivate_license'])) {
            delete_option('fbp_license_key'); delete_option('fbp_license_status'); delete_option('fbp_license_tier'); return;
        }
        $key = sanitize_text_field($_POST['fbp_license_key']);
        if (empty($key)) return;
        $result = $this->verify_license($key);
        if ($result['success']) {
            update_option('fbp_license_key', $key); update_option('fbp_license_status', 'valid'); update_option('fbp_license_tier', $result['tier']);
        } else {
            update_option('fbp_license_status', 'invalid');
        }
    }

    private function verify_license($key) {
        $r = wp_remote_post('https://api.gumroad.com/v2/licenses/verify', array('body' => array('product_permalink' => $this->product_permalink, 'license_key' => $key), 'timeout' => 15));
        if (is_wp_error($r)) return array('success' => false, 'message' => 'Connection failed.');
        $b = json_decode(wp_remote_retrieve_body($r), true);
        if (!isset($b['success']) || !$b['success']) return array('success' => false, 'message' => 'Invalid license.');
        $p = isset($b['purchase']) ? $b['purchase'] : array();
        $v = isset($p['variants']) && is_array($p['variants']) ? implode(', ', $p['variants']) : (isset($p['variants']) ? $p['variants'] : 'Basic');
        $t = 'Basic';
        if (stripos($v, 'enterprise') !== false) $t = 'Enterprise'; elseif (stripos($v, 'pro') !== false) $t = 'Pro';
        if (isset($p['chargebacked']) && $p['chargebacked']) return array('success' => false, 'message' => 'Refunded.');
        return array('success' => true, 'tier' => $t, 'data' => $p);
    }

    public function license_admin_notice() {
        $s = $this->get_license_status(); $c = get_current_screen();
        if ($c && $c->id === 'toplevel_page_form-builder-pro' && $s !== 'valid') {
            echo '<div class="notice notice-warning"><p><strong>Form Builder Pro:</strong> <a href="?page=fbp-settings">Activate license</a> to unlock all features.</p></div>';
        }
    }

    public function activate() {
        global $wpdb;
        $table_forms = $wpdb->prefix . 'fbp_forms';
        $table_submissions = $wpdb->prefix . 'fbp_submissions';
        $charset_collate = $wpdb->get_charset_collate();
        $sql = "CREATE TABLE IF NOT EXISTS $table_forms (
            id bigint(20) NOT NULL AUTO_INCREMENT,
            name varchar(100) NOT NULL,
            fields longtext NOT NULL,
            settings longtext NOT NULL,
            status varchar(20) DEFAULT 'active',
            created datetime DEFAULT CURRENT_TIMESTAMP,
            updated datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (id)
        ) $charset_collate;
        CREATE TABLE IF NOT EXISTS $table_submissions (
            id bigint(20) NOT NULL AUTO_INCREMENT,
            form_id bigint(20) NOT NULL,
            data longtext NOT NULL,
            ip_address varchar(45),
            user_agent varchar(255),
            status varchar(20) DEFAULT 'new',
            created datetime DEFAULT CURRENT_TIMESTAMP,
            PRIMARY KEY (id),
            KEY idx_form (form_id)
        ) $charset_collate;";
        require_once(ABSPATH . 'wp-admin/includes/upgrade.php');
        dbDelta($sql);
    }
    
    public function admin_menu() {
        add_menu_page('Form Builder Pro', 'Forms', 'manage_options', 'form-builder-pro', array($this, 'forms_page'), 'dashicons-feedback', 4);
        add_submenu_page('form-builder-pro', 'All Forms', 'All Forms', 'manage_options', 'form-builder-pro', array($this, 'forms_page'));
        add_submenu_page('form-builder-pro', 'Add New', 'Add New', 'manage_options', 'fbp-add-form', array($this, 'add_form_page'));
        add_submenu_page('form-builder-pro', 'Submissions', 'Submissions', 'manage_options', 'fbp-submissions', array($this, 'submissions_page'));
        add_submenu_page('form-builder-pro', 'Settings', 'Settings', 'manage_options', 'fbp-settings', array($this, 'settings_page'));
    }
    
    public function forms_page() {
        $forms = $this->get_forms();
        ?>
        <div class="wrap">
            <h1>📝 Form Builder Pro</h1>
            <a href="?page=fbp-add-form" class="button button-primary">+ Create New Form</a>
            <table class="wp-list-table widefat fixed striped" style="margin-top: 20px;">
                <thead><tr><th>Name</th><th>Shortcode</th><th>Submissions</th><th>Status</th><th>Created</th><th>Actions</th></tr></thead>
                <tbody>
                    <?php foreach ($forms as $form): ?>
                    <tr>
                        <td><strong><?php echo esc_html($form['name']); ?></strong></td>
                        <td><code>[digi_form id="<?php echo $form['id']; ?>"]</code></td>
                        <td><?php echo $this->get_submission_count($form['id']); ?></td>
                        <td><span style="color:<?php echo $form['status']==='active'?'green':'red'; ?>"><?php echo ucfirst($form['status']); ?></span></td>
                        <td><?php echo date('M j, Y', strtotime($form['created'])); ?></td>
                        <td><a href="?page=fbp-add-form&edit=<?php echo $form['id']; ?>">Edit</a> | <a href="?page=fbp-submissions&form=<?php echo $form['id']; ?>">View</a></td>
                    </tr>
                    <?php endforeach; ?>
                </tbody>
            </table>
        </div>
        <?php
    }
    
    public function add_form_page() {
        $form_id = $_GET['edit'] ?? null;
        $form = $form_id ? $this->get_form($form_id) : null;
        ?>
        <div class="wrap">
            <h1><?php echo $form ? 'Edit Form' : 'Create New Form'; ?></h1>
            <div class="fbp-builder">
                <div class="fbp-sidebar">
                    <h3>Elements</h3>
                    <div class="fbp-element" draggable="true" data-type="text">📝 Text</div>
                    <div class="fbp-element" draggable="true" data-type="email">📧 Email</div>
                    <div class="fbp-element" draggable="true" data-type="textarea">📄 Textarea</div>
                    <div class="fbp-element" draggable="true" data-type="select">📋 Dropdown</div>
                    <div class="fbp-element" draggable="true" data-type="checkbox">☑️ Checkbox</div>
                    <div class="fbp-element" draggable="true" data-type="radio">🔘 Radio</div>
                    <div class="fbp-element" draggable="true" data-type="number">🔢 Number</div>
                    <div class="fbp-element" draggable="true" data-type="phone">📱 Phone</div>
                    <div class="fbp-element" draggable="true" data-type="date">📅 Date</div>
                    <div class="fbp-element" draggable="true" data-type="file" style="<?php echo $this->has_feature('file_upload')?'':'opacity:0.5;'; ?>">📎 File Upload <?php echo $this->has_feature('file_upload')?'':'🔒'; ?></div>
                    <div class="fbp-element" draggable="true" data-type="recaptcha" style="<?php echo $this->has_feature('recaptcha')?'':'opacity:0.5;'; ?>">🤖 reCAPTCHA <?php echo $this->has_feature('recaptcha')?'':'🔒'; ?></div>
                </div>
                <div class="fbp-canvas">
                    <div class="fbp-form-preview" id="fbp-canvas"><div class="fbp-placeholder">Drag elements here</div></div>
                </div>
                <div class="fbp-settings-panel">
                    <h3>Settings</h3>
                    <form id="fbp-form-settings">
                        <input type="hidden" name="form_id" value="<?php echo $form_id; ?>">
                        <p><label>Form Name</label><br><input type="text" name="form_name" value="<?php echo esc_attr($form['name'] ?? ''); ?>" required></p>
                        <p><label>Button Text</label><br><input type="text" name="button_text" value="<?php echo esc_attr($form['settings']['button_text'] ?? 'Submit'); ?>"></p>
                        <p><label>Success Message</label><br><input type="text" name="success_message" value="<?php echo esc_attr($form['settings']['success_message'] ?? 'Thank you!'); ?>"></p>
                        <p><label>Email Notifications</label><br><input type="email" name="notification_email" value="<?php echo esc_attr($form['settings']['notification_email'] ?? get_option('admin_email')); ?>"></p>
                        <p><label><input type="checkbox" name="enable_spam_check" value="1" checked> Spam Protection</label></p>
                        <button type="submit" class="button button-primary">Save Form</button>
                    </form>
                </div>
            </div>
        </div>
        <style>
        .fbp-builder{display:flex;gap:20px;margin-top:20px;}
        .fbp-sidebar{width:200px;background:#fff;padding:15px;border-radius:8px;border:1px solid #ccd0d4;}
        .fbp-canvas{flex:1;background:#fff;padding:20px;border-radius:8px;border:1px solid #ccd0d4;min-height:400px;}
        .fbp-settings-panel{width:300px;background:#fff;padding:15px;border-radius:8px;border:1px solid #ccd0d4;}
        .fbp-element{padding:10px;margin:5px 0;background:#f0f0f1;border-radius:4px;cursor:grab;}
        .fbp-element:hover{background:#2271b1;color:#fff;}
        .fbp-placeholder{text-align:center;padding:40px;color:#999;}
        </style>
        <?php
    }
    
    public function submissions_page() {
        $form_id = $_GET['form'] ?? null;
        $submissions = $form_id ? $this->get_submissions($form_id) : $this->get_all_submissions();
        ?>
        <div class="wrap">
            <h1>📋 Submissions</h1>
            <table class="wp-list-table widefat fixed striped">
                <thead><tr><th>Form</th><th>Data</th><th>IP</th><th>Date</th></tr></thead>
                <tbody>
                    <?php foreach ($submissions as $s): ?>
                    <tr><td><?php echo $this->get_form_name($s['form_id']); ?></td><td><?php echo esc_html(substr($s['data'],0,80)); ?>...</td><td><?php echo $s['ip_address']; ?></td><td><?php echo date('M j, Y', strtotime($s['created'])); ?></td></tr>
                    <?php endforeach; ?>
                </tbody>
            </table>
            <?php if ($this->has_feature('csv_export')): ?><button class="button" onclick="alert('Export CSV')">📥 Export CSV</button><?php else: ?><p><em>CSV Export: Pro feature 🔒</em></p><?php endif; ?>
        </div>
        <?php
    }
    
    public function settings_page() {
        $license_key = $this->get_license_key();
        $license_status = $this->get_license_status();
        $license_tier = $this->get_license_tier();
        ?>
        <div class="wrap">
            <h1>⚙️ Settings</h1>
            <div style="background:#fff;border:1px solid #ccd0d4;border-radius:8px;padding:20px;margin:20px 0;">
                <h2>🔑 License</h2>
                <?php if ($license_status === 'valid'): ?>
                    <p><strong>Status:</strong> <span style="color:green;font-weight:bold;">ACTIVE</span> | <strong>Plan:</strong> <?php echo $license_tier; ?> | <strong>Sites:</strong> <?php echo $this->get_site_count()===-1?'Unlimited':$this->get_site_count(); ?></p>
                    <form method="post"><?php wp_nonce_field('fbp_license_action'); ?><button name="fbp_deactivate_license" class="button">Deactivate</button></form>
                <?php else: ?>
                    <form method="post"><?php wp_nonce_field('fbp_license_action'); ?><input type="text" name="fbp_license_key" placeholder="License key" style="width:300px;" required> <button name="fbp_activate_license" class="button button-primary">Activate</button></form>
                <?php endif; ?>
            </div>
            <form method="post">
                <?php wp_nonce_field('fbp_settings'); ?>
                <table class="form-table">
                    <tr><th>From Email</th><td><input type="email" name="from_email" value="<?php echo get_option('fbp_from_email', get_option('admin_email')); ?>"></td></tr>
                    <tr><th>Email Subject Prefix</th><td><input type="text" name="email_prefix" value="<?php echo get_option('fbp_email_prefix', '[Form]'); ?>"></td></tr>
                    <tr><th>reCAPTCHA Site Key</th><td><input type="text" name="recaptcha_key" value="<?php echo get_option('fbp_recaptcha_key', ''); ?>"></td></tr>
                    <tr><th>reCAPTCHA Secret</th><td><input type="password" name="recaptcha_secret" value="<?php echo get_option('fbp_recaptcha_secret', ''); ?>"></td></tr>
                </table>
                <?php submit_button(); ?>
            </form>
        </div>
        <?php
    }
    
    public function form_shortcode($atts) {
        $atts = shortcode_atts(array('id' => 0), $atts);
        $form = $this->get_form($atts['id']);
        if (!$form) return '<p>Form not found.</p>';
        $html = '<form class="fbp-form" data-form-id="'.$form['id'].'">';
        foreach ($form['fields'] as $field) {
            $html .= '<div class="fbp-field-wrap"><label>'.esc_html($field['label']??'').'</label>';
            switch ($field['type']) {
                case 'text': case 'email': case 'phone': case 'number': case 'date':
                    $html .= '<input type="'.$field['type'].'" name="'.($field['name']??'').'" placeholder="'.($field['placeholder']??'').'"'.(($field['required']??false)?' required':'').'>'; break;
                case 'textarea': $html .= '<textarea name="'.($field['name']??'').'" rows="4" placeholder="'.($field['placeholder']??'').'"'.(($field['required']??false)?' required':'').'></textarea>'; break;
                case 'select':
                    $html .= '<select name="'.($field['name']??'').'">';
                    foreach ($field['options']??[] as $o) $html .= '<option value="'.esc_attr($o).'">'.esc_html($o).'</option>';
                    $html .= '</select>'; break;
                case 'checkbox': case 'radio':
                    foreach ($field['options']??[] as $o) $html .= '<label><input type="'.$field['type'].'" name="'.($field['name']??'').'" value="'.esc_attr($o).'"> '.esc_html($o).'</label><br>'; break;
            }
            $html .= '</div>';
        }
        $html .= '<button type="submit" class="fbp-submit-btn">'.esc_html($form['settings']['button_text']??'Submit').'</button></form>';
        return $html;
    }
    
    public function ajax_save_form() {
        check_ajax_referer('fbp_nonce', 'nonce');
        global $wpdb;
        $form_id = $_POST['form_id'];
        $data = array(
            'name' => sanitize_text_field($_POST['form_name']),
            'fields' => json_encode($_POST['fields']),
            'settings' => json_encode(array('button_text'=>sanitize_text_field($_POST['button_text']),'success_message'=>sanitize_text_field($_POST['success_message']),'notification_email'=>sanitize_email($_POST['notification_email']),'enable_spam_check'=>!empty($_POST['enable_spam_check']))),
        );
        if ($form_id) $wpdb->update($wpdb->prefix.'fbp_forms', $data, array('id'=>$form_id));
        else { $wpdb->insert($wpdb->prefix.'fbp_forms', $data); $form_id = $wpdb->insert_id; }
        wp_send_json_success(array('form_id'=>$form_id));
    }
    
    public function ajax_submit_form() {
        check_ajax_referer('fbp_submit_nonce', 'nonce');
        global $wpdb;
        $form_id = intval($_POST['form_id']);
        $form = $this->get_form($form_id);
        if (!$form) wp_send_json_error('Not found');
        $wpdb->insert($wpdb->prefix.'fbp_submissions', array('form_id'=>$form_id,'data'=>json_encode($_POST['data']),'ip_address'=>$_SERVER['REMOTE_ADDR'],'user_agent'=>$_SERVER['HTTP_USER_AGENT']));
        if (!empty($form['settings']['notification_email'])) wp_mail($form['settings']['notification_email'], 'Form Submission: '.$form['name'], print_r($_POST['data'], true));
        wp_send_json_success(array('message'=>$form['settings']['success_message']??'Thank you!'));
    }
    
    private function get_forms() { global $wpdb; return $wpdb->get_results("SELECT * FROM {$wpdb->prefix}fbp_forms ORDER BY created DESC", ARRAY_A); }
    private function get_form($id) { global $wpdb; $f = $wpdb->get_row($wpdb->prepare("SELECT * FROM {$wpdb->prefix}fbp_forms WHERE id = %d",$id), ARRAY_A); if ($f) { $f['fields']=json_decode($f['fields'],true); $f['settings']=json_decode($f['settings'],true); } return $f; }
    private function get_submissions($id) { global $wpdb; return $wpdb->get_results($wpdb->prepare("SELECT * FROM {$wpdb->prefix}fbp_submissions WHERE form_id=%d ORDER BY created DESC",$id), ARRAY_A); }
    private function get_all_submissions() { global $wpdb; return $wpdb->get_results("SELECT * FROM {$wpdb->prefix}fbp_submissions ORDER BY created DESC LIMIT 100", ARRAY_A); }
    private function get_submission_count($id) { global $wpdb; return (int)$wpdb->get_var($wpdb->prepare("SELECT COUNT(*) FROM {$wpdb->prefix}fbp_submissions WHERE form_id=%d",$id)); }
    private function get_form_name($id) { global $wpdb; return $wpdb->get_var($wpdb->prepare("SELECT name FROM {$wpdb->prefix}fbp_forms WHERE id=%d",$id)); }
}

Form_Builder_Pro::instance();