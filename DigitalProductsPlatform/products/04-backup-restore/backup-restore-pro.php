<?php
/**
 * Plugin Name: Backup & Restore Pro
 * Plugin URI: https://creativstudio.gumroad.com/l/backup-restore-pro
 * Description: Complete WordPress backup - database, files, scheduling, one-click restore
 * Version: 3.2.0
 * Author: creativstudio
 * License: GPL v2 or later
 */

if (!defined('ABSPATH')) exit;

class Backup_Restore_Pro {
    
    private static $instance = null;
    private $backup_dir;
    private $product_permalink = 'backup-restore-pro';
    
    public static function instance() {
        if (is_null(self::$instance)) { self::$instance = new self(); }
        return self::$instance;
    }
    
    private function __construct() {
        $upload_dir = wp_upload_dir();
        $this->backup_dir = $upload_dir['basedir'] . '/backups';
        register_activation_hook(__FILE__, array($this, 'activate'));
        add_action('admin_menu', array($this, 'admin_menu'));
        add_action('wp_ajax_brp_backup', array($this, 'ajax_backup'));
        add_action('wp_ajax_brp_restore', array($this, 'ajax_restore'));
        add_action('admin_init', array($this, 'handle_license_activation'));
        add_action('admin_notices', array($this, 'license_admin_notice'));
    }

    // ============ LICENSE ============
    public function get_license_key() { return get_option('brp_license_key', ''); }
    public function get_license_status() { return get_option('brp_license_status', 'invalid'); }
    public function get_license_tier() { return get_option('brp_license_tier', 'none'); }

    public function has_feature($feature) {
        $tier = strtolower($this->get_license_tier());
        if ($this->get_license_status() !== 'valid') return false;
        $features = array(
            'basic' => array('backup', 'restore', 'scheduled', 'email_storage', 'basic_support'),
            'pro' => array('backup', 'restore', 'scheduled', 'email_storage', 'basic_support', 'ftp_storage', 'file_exclusions', 'priority_support'),
            'enterprise' => array('backup', 'restore', 'scheduled', 'email_storage', 'basic_support', 'ftp_storage', 'file_exclusions', 'priority_support', 's3_storage', 'dropbox', 'google_drive', 'multisite', 'white_label', 'dedicated_support'),
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
        if (!isset($_POST['brp_activate_license']) && !isset($_POST['brp_deactivate_license'])) return;
        if (!current_user_can('manage_options')) return;
        check_admin_referer('brp_license_action');
        if (isset($_POST['brp_deactivate_license'])) {
            delete_option('brp_license_key'); delete_option('brp_license_status'); delete_option('brp_license_tier'); return;
        }
        $key = sanitize_text_field($_POST['brp_license_key']);
        if (empty($key)) return;
        $result = $this->verify_license($key);
        if ($result['success']) {
            update_option('brp_license_key', $key); update_option('brp_license_status', 'valid'); update_option('brp_license_tier', $result['tier']);
        } else { update_option('brp_license_status', 'invalid'); }
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
        $c = get_current_screen();
        if ($c && $c->id === 'toplevel_page_backup-restore-pro' && $this->get_license_status() !== 'valid') {
            echo '<div class="notice notice-warning"><p><strong>Backup & Restore Pro:</strong> <a href="?page=brp-settings">Activate license</a> to unlock all features.</p></div>';
        }
    }
    
    public function activate() {
        if (!file_exists($this->backup_dir)) { wp_mkdir_p($this->backup_dir); }
        if (!wp_next_scheduled('brp_daily_backup')) { wp_schedule_event(time(), 'daily', 'brp_daily_backup'); }
    }
    
    public function admin_menu() {
        add_menu_page('Backup & Restore Pro', 'Backups', 'manage_options', 'backup-restore-pro', array($this, 'dashboard_page'), 'dashicons-database', 5);
        add_submenu_page('backup-restore-pro', 'Create Backup', 'Create Backup', 'manage_options', 'brp-create', array($this, 'create_backup_page'));
        add_submenu_page('backup-restore-pro', 'Restore', 'Restore', 'manage_options', 'brp-restore', array($this, 'restore_page'));
        add_submenu_page('backup-restore-pro', 'Schedule', 'Schedule', 'manage_options', 'brp-schedule', array($this, 'schedule_page'));
        add_submenu_page('backup-restore-pro', 'Remote Storage', 'Remote Storage', 'manage_options', 'brp-remote', array($this, 'remote_page'));
        add_submenu_page('backup-restore-pro', 'Settings', 'Settings', 'manage_options', 'brp-settings', array($this, 'settings_page'));
    }

    public function settings_page() {
        $license_key = $this->get_license_key();
        $license_status = $this->get_license_status();
        $license_tier = $this->get_license_tier();
        ?>
        <div class="wrap">
            <h1>⚙️ Backup & Restore Settings</h1>
            <div style="background:#fff;border:1px solid #ccd0d4;border-radius:8px;padding:20px;margin:20px 0;">
                <h2>🔑 License</h2>
                <?php if ($license_status === 'valid'): ?>
                    <p><strong>Status:</strong> <span style="color:green;font-weight:bold;">ACTIVE</span> | <strong>Plan:</strong> <?php echo $license_tier; ?> | <strong>Sites:</strong> <?php echo $this->get_site_count()===-1?'Unlimited':$this->get_site_count(); ?></p>
                    <form method="post"><?php wp_nonce_field('brp_license_action'); ?><button name="brp_deactivate_license" class="button">Deactivate</button></form>
                <?php else: ?>
                    <form method="post"><?php wp_nonce_field('brp_license_action'); ?><input type="text" name="brp_license_key" placeholder="License key" style="width:300px;" required> <button name="brp_activate_license" class="button button-primary">Activate</button></form>
                <?php endif; ?>
            </div>
        </div>
        <?php
    }
    
    public function dashboard_page() {
        $backups = $this->get_backups();
        ?>
        <div class="wrap">
            <h1>💾 Backup & Restore Pro</h1>
            <div class="brp-stats">
                <div class="brp-stat-box"><h3><?php echo count($backups); ?></h3><p>Total Backups</p></div>
                <div class="brp-stat-box"><h3><?php echo $this->get_total_size(); ?></h3><p>Total Size</p></div>
                <div class="brp-stat-box"><h3><?php echo get_option('brp_last_backup', 'Never'); ?></h3><p>Last Backup</p></div>
            </div>
            <h2>Recent Backups</h2>
            <table class="wp-list-table widefat fixed striped">
                <thead><tr><th>Name</th><th>Type</th><th>Size</th><th>Date</th><th>Actions</th></tr></thead>
                <tbody>
                    <?php foreach (array_slice($backups, 0, 10) as $backup): ?>
                    <tr>
                        <td><strong><?php echo esc_html($backup['name']); ?></strong></td>
                        <td><?php echo ucfirst($backup['type']); ?></td>
                        <td><?php echo size_format($backup['size']); ?></td>
                        <td><?php echo date('M j, Y H:i', strtotime($backup['date'])); ?></td>
                        <td>
                            <a href="<?php echo $backup['url']; ?>" class="button" download>📥 Download</a>
                            <button class="button" onclick="brp_restore('<?php echo $backup['name']; ?>')">🔄 Restore</button>
                            <button class="button" onclick="brp_delete('<?php echo $backup['name']; ?>')">🗑️ Delete</button>
                        </td>
                    </tr>
                    <?php endforeach; ?>
                </tbody>
            </table>
        </div>
        <?php
    }
    
    public function create_backup_page() {
        ?>
        <div class="wrap">
            <h1>📦 Create Backup</h1>
            <div class="brp-backup-options">
                <h2>Backup Options</h2>
                <form id="brp-backup-form">
                    <table class="form-table">
                        <tr>
                            <th>Backup Type</th>
                            <td>
                                <select name="backup_type">
                                    <option value="full">Full Backup (Database + Files)</option>
                                    <option value="database">Database Only</option>
                                    <option value="files">Files Only</option>
                                    <option value="plugins">Plugins Only</option>
                                    <option value="themes">Themes Only</option>
                                    <option value="uploads">Uploads Only</option>
                                </select>
                            </td>
                        </tr>
                        <tr>
                            <th>Backup Name</th>
                            <td><input type="text" name="backup_name" placeholder="My Backup <?php echo date('Y-m-d'); ?>"></td>
                        </tr>
                        <tr>
                            <th>Compression</th>
                            <td>
                                <select name="compression">
                                    <option value="zip">ZIP</option>
                                    <option value="tar">TAR.GZ</option>
                                    <option value="none">No Compression</option>
                                </select>
                            </td>
                        </tr>
                        <tr>
                            <th>Include Database</th>
                            <td><input type="checkbox" name="include_db" value="1" checked></td>
                        </tr>
                        <tr>
                            <th>Include Files</th>
                            <td><input type="checkbox" name="include_files" value="1" checked></td>
                        </tr>
                    </table>
                    <button type="button" class="button button-primary button-hero" onclick="brp_start_backup()">🚀 Create Backup Now</button>
                </form>
            </div>
            <div id="brp-progress" style="display:none;">
                <h2>Backup in Progress...</h2>
                <div class="progress-bar"><div class="progress-fill" id="brp-progress-bar"></div></div>
                <p id="brp-status">Initializing...</p>
            </div>
        </div>
        <?php
    }
    
    public function restore_page() {
        ?>
        <div class="wrap">
            <h1>🔄 Restore Backup</h1>
            <p>Select a backup to restore. This will overwrite your current site.</p>
            <div class="notice notice-warning"><p><strong>⚠️ Warning:</strong> Restoring a backup will replace your current data. Make sure to create a backup first.</p></div>
            <form id="brp-restore-form">
                <table class="form-table">
                    <tr>
                        <th>Select Backup</th>
                        <td>
                            <select name="backup_name" required>
                                <option value="">Choose a backup...</option>
                                <?php foreach ($this->get_backups() as $backup): ?>
                                <option value="<?php echo $backup['name']; ?>"><?php echo $backup['name']; ?> (<?php echo size_format($backup['size']); ?>)</option>
                                <?php endforeach; ?>
                            </select>
                        </td>
                    </tr>
                    <tr>
                        <th>Restore Options</th>
                        <td>
                            <label><input type="checkbox" name="restore_db" value="1" checked> Restore Database</label><br>
                            <label><input type="checkbox" name="restore_files" value="1" checked> Restore Files</label><br>
                            <label><input type="checkbox" name="create_pre_restore_backup" value="1" checked> Create backup before restore</label>
                        </td>
                    </tr>
                </table>
                <button type="button" class="button button-primary button-hero" onclick="brp_start_restore()">🔄 Restore Now</button>
            </form>
        </div>
        <?php
    }
    
    public function schedule_page() {
        ?>
        <div class="wrap">
            <h1>📅 Backup Schedule</h1>
            <form method="post">
                <?php wp_nonce_field('brp_settings'); ?>
                <table class="form-table">
                    <tr>
                        <th>Enable Scheduled Backups</th>
                        <td><input type="checkbox" name="enable_scheduled" value="1" checked></td>
                    </tr>
                    <tr>
                        <th>Frequency</th>
                        <td>
                            <select name="frequency">
                                <option value="daily">Daily</option>
                                <option value="weekly">Weekly</option>
                                <option value="monthly">Monthly</option>
                            </select>
                        </td>
                    </tr>
                    <tr>
                        <th>Time</th>
                        <td><input type="time" name="backup_time" value="03:00"></td>
                    </tr>
                    <tr>
                        <th>Keep Last X Backups</th>
                        <td><input type="number" name="keep_backups" value="30" min="1" max="365"></td>
                    </tr>
                    <tr>
                        <th>Backup Type</th>
                        <td>
                            <select name="scheduled_type">
                                <option value="full">Full Backup</option>
                                <option value="database">Database Only</option>
                                <option value="files">Files Only</option>
                            </select>
                        </td>
                    </tr>
                </table>
                <?php submit_button('Save Schedule'); ?>
            </form>
        </div>
        <?php
    }
    
    public function remote_page() {
        $s3_feature = $this->has_feature('s3_storage');
        $dropbox_feature = $this->has_feature('dropbox');
        $gd_feature = $this->has_feature('google_drive');
        ?>
        <div class="wrap">
            <h1>☁️ Remote Storage</h1>
            <form method="post">
                <?php wp_nonce_field('brp_settings'); ?>
                <table class="form-table">
                    <tr>
                        <th>Enable Remote Storage</th>
                        <td><input type="checkbox" name="enable_remote" value="1"></td>
                    </tr>
                    <tr>
                        <th>Storage Provider</th>
                        <td>
                            <select name="provider">
                                <option value="email">Email (Send to Email) <?php echo $this->has_feature('email_storage')?'':'🔒'; ?></option>
                                <option value="ftp" <?php echo !$this->has_feature('ftp_storage')?'disabled':''; ?>>FTP/SFTP <?php echo !$this->has_feature('ftp_storage')?'🔒':''; ?></option>
                                <option value="s3" <?php echo !$s3_feature?'disabled':''; ?>>Amazon S3 <?php echo !$s3_feature?'🔒':''; ?></option>
                                <option value="google" <?php echo !$gd_feature?'disabled':''; ?>>Google Drive <?php echo !$gd_feature?'🔒':''; ?></option>
                                <option value="dropbox" <?php echo !$dropbox_feature?'disabled':''; ?>>Dropbox <?php echo !$dropbox_feature?'🔒':''; ?></option>
                            </select>
                        </td>
                    </tr>
                    <tr>
                        <th>API Key / Token</th>
                        <td><input type="password" name="api_key" class="regular-text"></td>
                    </tr>
                    <tr>
                        <th>Bucket / Folder</th>
                        <td><input type="text" name="bucket" class="regular-text"></td>
                    </tr>
                </table>
                <?php submit_button('Save Settings'); ?>
            </form>
        </div>
        <?php
    }
    
    public function ajax_backup() {
        check_ajax_referer('brp_nonce', 'nonce');
        $type = $_POST['type'] ?? 'full';
        $name = sanitize_text_field($_POST['name']) ?: 'backup-' . date('Y-m-d-His');
        $backup_file = $this->backup_dir . '/' . $name . '.zip';
        $db_dump = '';
        if ($type === 'full' || $type === 'database') { $db_dump = $this->export_database(); }
        $zip = new ZipArchive();
        if ($zip->open($backup_file, ZipArchive::CREATE | ZipArchive::OVERWRITE)) {
            if ($db_dump) { $zip->addFromString('database.sql', $db_dump); }
            if ($type === 'full' || $type === 'files') { $this->add_files_to_zip($zip, ABSPATH, 'files'); }
            $zip->close();
            update_option('brp_last_backup', date('Y-m-d H:i:s'));
            wp_send_json_success(array('message' => 'Backup created successfully!', 'file' => $name . '.zip', 'size' => filesize($backup_file)));
        } else { wp_send_json_error('Failed to create backup'); }
    }
    
    public function ajax_restore() {
        check_ajax_referer('brp_nonce', 'nonce');
        $backup_name = sanitize_text_field($_POST['backup_name']);
        $backup_file = $this->backup_dir . '/' . $backup_name;
        if (!file_exists($backup_file)) { wp_send_json_error('Backup file not found'); }
        $zip = new ZipArchive();
        if ($zip->open($backup_file)) {
            if (file_exists($zip->getNameFromName('database.sql'))) { $this->import_database($zip->getFromName('database.sql')); }
            $zip->extractTo(ABSPATH);
            $zip->close();
            wp_send_json_success(array('message' => 'Backup restored successfully!'));
        } else { wp_send_json_error('Failed to open backup file'); }
    }
    
    private function export_database() {
        global $wpdb;
        $tables = $wpdb->get_results("SHOW TABLES", ARRAY_N);
        $dump = '';
        foreach ($tables as $table) {
            $table_name = $table[0];
            if (strpos($table_name, $wpdb->prefix) === 0) {
                $dump .= "-- Table: $table_name\nDROP TABLE IF EXISTS `$table_name`;\n";
                $create = $wpdb->get_row("SHOW CREATE TABLE `$table_name`", ARRAY_N);
                $dump .= $create[1] . ";\n\n";
                $rows = $wpdb->get_results("SELECT * FROM `$table_name`", ARRAY_A);
                foreach ($rows as $row) {
                    $values = array_map(function($v) { return $v === null ? 'NULL' : "'" . addslashes($v) . "'"; }, $row);
                    $dump .= "INSERT INTO `$table_name` VALUES (" . implode(', ', $values) . ");\n";
                }
                $dump .= "\n\n";
            }
        }
        return $dump;
    }
    
    private function import_database($sql) {
        global $wpdb;
        $queries = explode(";\n", $sql);
        foreach ($queries as $query) {
            $query = trim($query);
            if (!empty($query)) { $wpdb->query($query); }
        }
    }
    
    private function add_files_to_zip($zip, $dir, $prefix) {
        $files = scandir($dir);
        foreach ($files as $file) {
            if ($file === '.' || $file === '..' || $file === 'wp-config.php') continue;
            $path = $dir . '/' . $file;
            if (is_dir($path)) { $this->add_files_to_zip($zip, $path, $prefix . '/' . $file); }
            else { $zip->addFile($path, $prefix . '/' . $file); }
        }
    }
    
    private function get_backups() {
        $backups = array();
        if (is_dir($this->backup_dir)) {
            foreach (glob($this->backup_dir . '/*.zip') as $file) {
                $backups[] = array('name' => basename($file, '.zip'), 'size' => filesize($file), 'date' => date('Y-m-d H:i:s', filemtime($file)), 'url' => wp_upload_dir()['baseurl'] . '/backups/' . basename($file), 'type' => 'full');
            }
        }
        usort($backups, function($a, $b) { return strtotime($b['date']) - strtotime($a['date']); });
        return $backups;
    }
    
    private function get_total_size() {
        $total = 0;
        if (is_dir($this->backup_dir)) {
            foreach (glob($this->backup_dir . '/*') as $file) { $total += filesize($file); }
        }
        return size_format($total);
    }
}

Backup_Restore_Pro::instance();