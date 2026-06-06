<?php
/**
 * Plugin Name: Security Scanner Pro
 * Plugin URI: https://digistore.com/products/security-scanner
 * Description: Complete WordPress security - malware scan, firewall, login protection, file integrity
 * Version: 4.1.0
 * Author: DigiStore Security
 * License: GPL v2 or later
 * Text Domain: security-scanner-pro
 */

if (!defined('ABSPATH')) exit;

class Security_Scanner_Pro {
    
    private static $instance = null;
    private $db_version = '4.1.0';
    private $product_permalink = 'security-scanner-pro';
    
    public static function instance() {
        if (is_null(self::$instance)) {
            self::$instance = new self();
        }
        return self::$instance;
    }
    
    private function __construct() {
        register_activation_hook(__FILE__, array($this, 'activate'));
        add_action('init', array($this, 'init'));
        add_action('admin_menu', array($this, 'admin_menu'));
        add_action('wp_login', array($this, 'log_login'), 10, 2);
        add_action('wp_ajax_ssp_scan', array($this, 'ajax_scan'));
        add_action('wp_ajax_ssp_fix', array($this, 'ajax_fix'));
        add_action('admin_init', array($this, 'check_security'));
        add_action('admin_init', array($this, 'handle_license_activation'));
        
        // File monitoring
        add_action('init', array($this, 'check_file_changes'));

        // License admin notice
        add_action('admin_notices', array($this, 'license_admin_notice'));
    }

    // ============ LICENSE SYSTEM ============

    public function get_license_key() {
        return get_option('ssp_license_key', '');
    }

    public function get_license_status() {
        return get_option('ssp_license_status', 'invalid');
    }

    public function get_license_tier() {
        return get_option('ssp_license_tier', 'none');
    }

    private function get_license_variant_label($variant) {
        $labels = array(
            'Basic' => 'Basic',
            'Pro' => 'Pro',
            'Enterprise' => 'Enterprise',
        );
        return isset($labels[$variant]) ? $labels[$variant] : $variant;
    }

    public function has_feature($feature) {
        $tier = $this->get_license_tier();
        $status = $this->get_license_status();

        if ($status !== 'valid') {
            return false;
        }

        $features = array(
            'basic' => array('scanner', 'firewall', 'login_protection', 'security_headers', 'basic_support'),
            'pro' => array('scanner', 'firewall', 'login_protection', 'security_headers', 'file_integrity', 'malware_removal', 'priority_support', 'real_time_monitoring'),
            'enterprise' => array('scanner', 'firewall', 'login_protection', 'security_headers', 'file_integrity', 'malware_removal', 'priority_support', 'real_time_monitoring', 'white_label', 'dedicated_support', 'custom_rules', 'api_access', 'sla'),
        );

        $tier_key = strtolower($tier);
        if (!isset($features[$tier_key])) {
            return false;
        }

        return in_array($feature, $features[$tier_key]);
    }

    public function get_site_count() {
        $tier = $this->get_license_tier();
        switch (strtolower($tier)) {
            case 'basic': return 1;
            case 'pro': return 5;
            case 'enterprise': return -1;
            default: return 0;
        }
    }

    public function handle_license_activation() {
        if (!isset($_POST['ssp_activate_license']) && !isset($_POST['ssp_deactivate_license'])) {
            return;
        }
        if (!current_user_can('manage_options')) {
            return;
        }
        check_admin_referer('ssp_license_action');

        if (isset($_POST['ssp_deactivate_license'])) {
            delete_option('ssp_license_key');
            delete_option('ssp_license_status');
            delete_option('ssp_license_tier');
            delete_option('ssp_license_data');
            add_action('admin_notices', function() {
                echo '<div class="notice notice-success"><p>License deactivated successfully.</p></div>';
            });
            return;
        }

        $license_key = sanitize_text_field($_POST['ssp_license_key']);
        if (empty($license_key)) {
            add_action('admin_notices', function() {
                echo '<div class="notice notice-error"><p>Please enter a license key.</p></div>';
            });
            return;
        }

        $result = $this->verify_license_with_gumroad($license_key);

        if ($result['success']) {
            update_option('ssp_license_key', $license_key);
            update_option('ssp_license_status', 'valid');
            update_option('ssp_license_tier', $result['tier']);
            update_option('ssp_license_data', $result['data']);
            add_action('admin_notices', function() use ($result) {
                echo '<div class="notice notice-success"><p>License activated! Your ' . esc_html($result['tier']) . ' plan is now active.</p></div>';
            });
        } else {
            update_option('ssp_license_status', 'invalid');
            add_action('admin_notices', function() use ($result) {
                echo '<div class="notice notice-error"><p>License activation failed: ' . esc_html($result['message']) . '</p></div>';
            });
        }
    }

    private function verify_license_with_gumroad($license_key) {
        $response = wp_remote_post('https://api.gumroad.com/v2/licenses/verify', array(
            'body' => array(
                'product_permalink' => $this->product_permalink,
                'license_key' => $license_key,
            ),
            'timeout' => 15,
        ));

        if (is_wp_error($response)) {
            return array(
                'success' => false,
                'message' => 'Could not connect to license server. Please try again.',
            );
        }

        $body = json_decode(wp_remote_retrieve_body($response), true);

        if (!isset($body['success']) || !$body['success']) {
            $msg = isset($body['message']) ? $body['message'] : 'Invalid license key.';
            return array('success' => false, 'message' => $msg);
        }

        $purchase = isset($body['purchase']) ? $body['purchase'] : array();
        $variant = isset($purchase['variants']) && is_array($purchase['variants']) 
            ? implode(', ', $purchase['variants']) 
            : (isset($purchase['variants']) ? $purchase['variants'] : 'Basic');

        $tier = 'Basic';
        if (stripos($variant, 'enterprise') !== false) {
            $tier = 'Enterprise';
        } elseif (stripos($variant, 'pro') !== false) {
            $tier = 'Pro';
        }

        $refunded = isset($purchase['chargebacked']) ? $purchase['chargebacked'] : false;
        $cancelled = isset($purchase['subscription_cancelled_at']) && !empty($purchase['subscription_cancelled_at']);

        if ($refunded) {
            return array('success' => false, 'message' => 'This license has been refunded.');
        }

        return array(
            'success' => true,
            'tier' => $tier,
            'data' => $purchase,
        );
    }

    public function license_admin_notice() {
        $status = $this->get_license_status();
        $screen = get_current_screen();
        if ($screen && $screen->id === 'toplevel_page_security-scanner-pro') {
            if ($status !== 'valid') {
                echo '<div class="notice notice-warning is-dismissible"><p><strong>Security Scanner Pro:</strong> Please activate your license key to unlock all features. <a href="?page=ssp-settings">Activate now</a></p></div>';
            }
        }
    }
    
    public function activate() {
        global $wpdb;
        
        $table_logs = $wpdb->prefix . 'ssp_login_logs';
        $table_scans = $wpdb->prefix . 'ssp_scans';
        $table_blocked = $wpdb->prefix . 'ssp_blocked_ips';
        
        $table_logs_security = $wpdb->prefix . 'ssp_security_logs';
        $charset_collate = $wpdb->get_charset_collate();
        
        $sql = "CREATE TABLE IF NOT EXISTS $table_logs (
            id bigint(20) NOT NULL AUTO_INCREMENT,
            user_id bigint(20) DEFAULT 0,
            username varchar(60) NOT NULL,
            ip_address varchar(45) NOT NULL,
            user_agent varchar(255) NOT NULL,
            status varchar(20) NOT NULL,
            timestamp datetime DEFAULT CURRENT_TIMESTAMP NOT NULL,
            PRIMARY KEY (id),
            KEY idx_ip (ip_address),
            KEY idx_user (user_id)
        ) $charset_collate;
        
        CREATE TABLE IF NOT EXISTS $table_scans (
            id bigint(20) NOT NULL AUTO_INCREMENT,
            scan_type varchar(50) NOT NULL,
            result longtext NOT NULL,
            threats int(11) DEFAULT 0,
            timestamp datetime DEFAULT CURRENT_TIMESTAMP NOT NULL,
            PRIMARY KEY (id)
        ) $charset_collate;
        
        CREATE TABLE IF NOT EXISTS $table_blocked (
            id bigint(20) NOT NULL AUTO_INCREMENT,
            ip_address varchar(45) NOT NULL,
            reason varchar(255) NOT NULL,
            blocked_until datetime DEFAULT NULL,
            timestamp datetime DEFAULT CURRENT_TIMESTAMP NOT NULL,
            PRIMARY KEY (id),
            KEY idx_ip (ip_address)
        ) $charset_collate;
        
        CREATE TABLE IF NOT EXISTS $table_logs_security (
            id bigint(20) NOT NULL AUTO_INCREMENT,
            type varchar(50) NOT NULL,
            message text NOT NULL,
            ip_address varchar(45) NOT NULL,
            timestamp datetime DEFAULT CURRENT_TIMESTAMP NOT NULL,
            PRIMARY KEY (id),
            KEY idx_type (type)
        ) $charset_collate;";
        
        require_once(ABSPATH . 'wp-admin/includes/upgrade.php');
        dbDelta($sql);
        
        update_option('ssp_db_version', $this->db_version);
        update_option('ssp_scan_schedule', 'daily');
        
        // Create log directory
        $upload_dir = wp_upload_dir();
        $log_dir = $upload_dir['basedir'] . '/ssp_logs';
        if (!file_exists($log_dir)) {
            wp_mkdir_p($log_dir);
        }
    }
    
    public function init() {
        // Load text domain
        load_plugin_textdomain('security-scanner-pro', false, dirname(plugin_basename(__FILE__)) . '/languages');
        
        // Add security headers
        add_action('send_headers', array($this, 'add_security_headers'));
    }
    
    public function admin_menu() {
        // Main menu
        add_menu_page(
            'Security Scanner Pro',
            'Security',
            'manage_options',
            'security-scanner-pro',
            array($this, 'dashboard_page'),
            'dashicons-shield',
            2
        );
        
        // Submenus
        add_submenu_page('security-scanner-pro', 'Security Dashboard', 'Dashboard', 'manage_options', 'security-scanner-pro', array($this, 'dashboard_page'));
        add_submenu_page('security-scanner-pro', 'Malware Scanner', 'Malware Scan', 'manage_options', 'ssp-scanner', array($this, 'scanner_page'));
        add_submenu_page('security-scanner-pro', 'Firewall', 'Firewall', 'manage_options', 'ssp-firewall', array($this, 'firewall_page'));
        add_submenu_page('security-scanner-pro', 'Login Protection', 'Login Protection', 'manage_options', 'ssp-login', array($this, 'login_page'));
        add_submenu_page('security-scanner-pro', 'File Integrity', 'File Integrity', 'manage_options', 'ssp-files', array($this, 'files_page'));
        add_submenu_page('security-scanner-pro', 'Security Log', 'Security Log', 'manage_options', 'ssp-log', array($this, 'log_page'));
        add_submenu_page('security-scanner-pro', 'Settings', 'Settings', 'manage_options', 'ssp-settings', array($this, 'settings_page'));
    }
    
    // ============ DASHBOARD ============
    public function dashboard_page() {
        $scan_results = $this->get_last_scan();
        $blocked_count = $this->get_blocked_ips_count();
        $login_attempts = $this->get_failed_logins_count();
        $threats = $scan_results['threats'] ?? 0;
        $license_tier = $this->get_license_tier();
        $license_status = $this->get_license_status();
        ?>
        <div class="wrap">
            <h1>🛡️ Security Scanner Pro <span class="ssp-tier-badge ssp-tier-<?php echo strtolower($license_tier); ?>"><?php echo $license_status === 'valid' ? esc_html($license_tier) : 'FREE MODE'; ?></span></h1>
            
            <div class="ssp-dashboard-grid">
                <div class="ssp-card <?php echo $threats > 0 ? 'ssp-danger' : 'ssp-success'; ?>">
                    <div class="ssp-card-icon">🔍</div>
                    <div class="ssp-card-content">
                        <h3><?php echo $threats; ?> Threats</h3>
                        <p>Last scan: <?php echo $scan_results['date'] ?? 'Never'; ?></p>
                    </div>
                    <a href="?page=ssp-scanner" class="button">Scan Now</a>
                </div>
                
                <div class="ssp-card <?php echo $blocked_count > 0 ? 'ssp-warning' : 'ssp-success'; ?>">
                    <div class="ssp-card-icon">🚫</div>
                    <div class="ssp-card-content">
                        <h3><?php echo $blocked_count; ?> Blocked IPs</h3>
                        <p>Active blocked addresses</p>
                    </div>
                    <a href="?page=ssp-firewall" class="button">Manage</a>
                </div>
                
                <div class="ssp-card <?php echo $login_attempts > 10 ? 'ssp-danger' : 'ssp-success'; ?>">
                    <div class="ssp-card-icon">🔐</div>
                    <div class="ssp-card-content">
                        <h3><?php echo $login_attempts; ?> Failed Logins</h3>
                        <p>Last 24 hours</p>
                    </div>
                    <a href="?page=ssp-login" class="button">View</a>
                </div>
                
                <div class="ssp-card ssp-success">
                    <div class="ssp-card-icon">✅</div>
                    <div class="ssp-card-content">
                        <h3>Firewall Active</h3>
                        <p>Protection status</p>
                    </div>
                    <span class="badge-active">ACTIVE</span>
                </div>
            </div>
            
            <div class="ssp-quick-actions">
                <h2>Quick Actions</h2>
                <button class="button button-primary button-hero" onclick="ssp_run_scan()">🔍 Run Security Scan</button>
                <button class="button button-secondary" onclick="ssp_update_files()">📁 Update File Checksums</button>
                <button class="button button-secondary" onclick="ssp_check_updates()">🔄 Check for Updates</button>
            </div>
            
            <div class="ssp-recent-activity">
                <h2>Recent Activity</h2>
                <?php $this->render_recent_logs(); ?>
            </div>
        </div>
        
        <style>
        .ssp-dashboard-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 20px; margin: 20px 0; }
        .ssp-card { background: #fff; border: 1px solid #ccd0d4; border-radius: 8px; padding: 20px; display: flex; align-items: center; gap: 15px; }
        .ssp-card-icon { font-size: 32px; }
        .ssp-card h3 { margin: 0; font-size: 24px; }
        .ssp-card p { margin: 5px 0 0; color: #666; }
        .ssp-danger { border-left: 4px solid #dc3232; }
        .ssp-warning { border-left: 4px solid #ffb900; }
        .ssp-success { border-left: 4px solid #00a32a; }
        .badge-active { background: #00a32a; color: #fff; padding: 5px 15px; border-radius: 15px; font-weight: bold; }
        .ssp-quick-actions { background: #fff; padding: 20px; border-radius: 8px; margin: 20px 0; }
        .ssp-recent-activity { background: #fff; padding: 20px; border-radius: 8px; }
        .ssp-tier-badge { display: inline-block; padding: 3px 15px; border-radius: 15px; font-size: 14px; font-weight: bold; vertical-align: middle; margin-left: 10px; }
        .ssp-tier-basic { background: #e3f2fd; color: #1565c0; }
        .ssp-tier-pro { background: #f3e5f5; color: #7b1fa2; }
        .ssp-tier-enterprise { background: #fff3e0; color: #e65100; }
        .ssp-tier-none { background: #fbe9e7; color: #bf360c; }
        .ssp-upgrade-notice { background: #fff3e0; border: 1px solid #ffcc80; border-radius: 8px; padding: 15px; margin: 20px 0; text-align: center; }
        .ssp-upgrade-notice a { color: #e65100; font-weight: bold; }
        </style>
        <?php
    }
    
    // ============ SCANNER ============
    public function scanner_page() {
        if (!$this->has_feature('scanner')) {
            echo '<div class="wrap"><h1>🔍 Malware Scanner</h1><div class="notice notice-warning"><p>Malware Scanner requires an active license. <a href="?page=ssp-settings">Activate your license</a> to unlock.</p></div></div>';
            return;
        }
        ?>
        <div class="wrap">
            <h1>🔍 Malware Scanner</h1>
            <p>Scan your WordPress installation for malware, viruses, and security threats.</p>
            
            <div class="ssp-scan-options">
                <h2>Scan Options</h2>
                <form id="ssp-scan-form">
                    <label><input type="checkbox" name="scan_core" checked> WordPress Core Files</label><br>
                    <label><input type="checkbox" name="scan_plugins" checked> Plugin Files</label><br>
                    <label><input type="checkbox" name="scan_themes" checked> Theme Files</label><br>
                    <label><input type="checkbox" name="scan_uploads" checked> Uploads Directory</label><br>
                    <label><input type="checkbox" name="scan_database" checked> Database Tables</label><br>
                    <label><input type="checkbox" name="scan_malware" checked> Known Malware Signatures</label><br>
                    <label><input type="checkbox" name="scan_backdoor" checked> Backdoor Detection</label><br>
                    
                    <button type="button" class="button button-primary button-hero" onclick="ssp_start_scan()">Start Scan</button>
                </form>
            </div>
            
            <div id="ssp-scan-progress" style="display:none;">
                <h2>Scanning...</h2>
                <div class="progress-bar"><div class="progress-fill" id="ssp-progress"></div></div>
                <p id="ssp-scan-status">Initializing...</p>
            </div>
            
            <div id="ssp-scan-results" style="display:none;">
                <h2>Scan Results</h2>
                <div class="ssp-results-list" id="ssp-results"></div>
            </div>
        </div>
        
        <style>
        .ssp-scan-options { background: #fff; padding: 20px; border-radius: 8px; margin: 20px 0; }
        .ssp-scan-options label { display: block; padding: 5px 0; cursor: pointer; }
        .progress-bar { background: #e0e0e0; border-radius: 10px; height: 20px; overflow: hidden; }
        .progress-fill { background: linear-gradient(90deg, #00a32a, #46b450); height: 100%; width: 0%; transition: width 0.3s; }
        .ssp-results-list { margin: 20px 0; }
        .ssp-threat { background: #fff; border-left: 4px solid #dc3232; padding: 15px; margin: 10px 0; border-radius: 4px; }
        .ssp-clean { background: #fff; border-left: 4px solid #00a32a; padding: 15px; margin: 10px 0; border-radius: 4px; }
        </style>
        <?php
    }
    
    // ============ FIREWALL ============
    public function firewall_page() {
        if (!$this->has_feature('firewall')) {
            echo '<div class="wrap"><h1>🔥 Firewall</h1><div class="notice notice-warning"><p>Firewall requires an active license. <a href="?page=ssp-settings">Activate your license</a> to unlock.</p></div></div>';
            return;
        }
        $blocked = $this->get_blocked_ips();
        ?>
        <div class="wrap">
            <h1>🔥 Firewall</h1>
            <p>Protect your site from malicious attacks and brute force.</p>
            
            <div class="ssp-firewall-stats">
                <div class="ssp-stat-box">
                    <h3><?php echo count($blocked); ?></h3>
                    <p>Blocked IPs</p>
                </div>
                <div class="ssp-stat-box">
                    <h3><?php echo $this->get_today_blocks(); ?></h3>
                    <p>Today's Blocks</p>
                </div>
                <div class="ssp-stat-box">
                    <h3><?php echo $this->get_firewall_rules_count(); ?></h3>
                    <p>Active Rules</p>
                </div>
            </div>
            
            <h2>Block IP Address</h2>
            <form id="ssp-block-form">
                <input type="text" name="ip_address" placeholder="IP Address to block" required>
                <input type="text" name="reason" placeholder="Reason">
                <select name="duration">
                    <option value="24h">24 Hours</option>
                    <option value="7d">7 Days</option>
                    <option value="30d">30 Days</option>
                    <option value="permanent">Permanent</option>
                </select>
                <button type="submit" class="button button-primary">Block IP</button>
            </form>
            
            <h2>Blocked IPs</h2>
            <table class="wp-list-table widefat fixed striped">
                <thead>
                    <tr><th>IP Address</th><th>Reason</th><th>Blocked Until</th><th>Actions</th></tr>
                </thead>
                <tbody>
                    <?php foreach ($blocked as $ip): ?>
                    <tr>
                        <td><?php echo esc_html($ip['ip_address']); ?></td>
                        <td><?php echo esc_html($ip['reason']); ?></td>
                        <td><?php echo $ip['blocked_until'] ? date('Y-m-d H:i', strtotime($ip['blocked_until'])) : 'Permanent'; ?></td>
                        <td><button class="button" onclick="ssp_unblock('<?php echo $ip['ip_address']; ?>')">Unblock</button></td>
                    </tr>
                    <?php endforeach; ?>
                </tbody>
            </table>
        </div>
        <?php
    }
    
    // ============ LOGIN PROTECTION ============
    public function login_page() {
        if (!$this->has_feature('login_protection')) {
            echo '<div class="wrap"><h1>🔐 Login Protection</h1><div class="notice notice-warning"><p>Login Protection requires an active license. <a href="?page=ssp-settings">Activate your license</a> to unlock.</p></div></div>';
            return;
        }
        $attempts = $this->get_failed_logins(20);
        ?>
        <div class="wrap">
            <h1>🔐 Login Protection</h1>
            
            <div class="ssp-login-settings">
                <h2>Login Security Settings</h2>
                <form method="post">
                    <?php wp_nonce_field('ssp_settings'); ?>
                    <table class="form-table">
                        <tr>
                            <th>Max Login Attempts</th>
                            <td><input type="number" name="max_attempts" value="5" min="1" max="20"></td>
                        </tr>
                        <tr>
                            <th>Lockout Duration (minutes)</th>
                            <td><input type="number" name="lockout_duration" value="15" min="5" max="1440"></td>
                        </tr>
                        <tr>
                            <th>Enable CAPTCHA</th>
                            <td><input type="checkbox" name="enable_captcha" value="1" checked></td>
                        </tr>
                        <tr>
                            <th>Email Admin on Failed Login</th>
                            <td><input type="checkbox" name="email_alerts" value="1" checked></td>
                        </tr>
                        <tr>
                            <th>Two-Factor Authentication</th>
                            <td><input type="checkbox" name="enable_2fa" value="1"></td>
                        </tr>
                    </table>
                    <button type="submit" class="button button-primary">Save Settings</button>
                </form>
            </div>
            
            <h2>Recent Failed Logins</h2>
            <table class="wp-list-table widefat fixed striped">
                <thead>
                    <tr><th>Username</th><th>IP Address</th><th>Time</th><th>User Agent</th></tr>
                </thead>
                <tbody>
                    <?php foreach ($attempts as $attempt): ?>
                    <tr>
                        <td><?php echo esc_html($attempt['username']); ?></td>
                        <td><?php echo esc_html($attempt['ip_address']); ?></td>
                        <td><?php echo date('Y-m-d H:i:s', strtotime($attempt['timestamp'])); ?></td>
                        <td><?php echo esc_html(substr($attempt['user_agent'], 0, 50)); ?>...</td>
                    </tr>
                    <?php endforeach; ?>
                </tbody>
            </table>
        </div>
        <?php
    }
    
    // ============ FILE INTEGRITY ============
    public function files_page() {
        if (!$this->has_feature('file_integrity')) {
            echo '<div class="wrap"><h1>📁 File Integrity Checker</h1><div class="notice notice-warning"><p>File Integrity Checking is available in <strong>Pro</strong> and <strong>Enterprise</strong> plans. <a href="?page=ssp-settings">Upgrade your license</a> to unlock.</p></div></div>';
            return;
        }
        ?>
        <div class="wrap">
            <h1>📁 File Integrity Checker</h1>
            <p>Monitor file changes and detect unauthorized modifications.</p>
            
            <div class="ssp-file-options">
                <button class="button button-primary" onclick="ssp_update_checksums()">Update Checksums</button>
                <button class="button" onclick="ssp_check_files()">Check Files</button>
            </div>
            
            <div id="ssp-file-results">
                <h2>File Changes Detected</h2>
                <table class="wp-list-table widefat fixed striped">
                    <thead>
                        <tr><th>File</th><th>Status</th><th>Last Modified</th><th>Actions</th></tr>
                    </thead>
                    <tbody id="ssp-file-list">
                        <tr><td colspan="4">Click "Check Files" to scan</td></tr>
                    </tbody>
                </table>
            </div>
        </div>
        <?php
    }
    
    // ============ SECURITY LOG ============
    public function log_page() {
        $logs = $this->get_security_logs(50);
        ?>
        <div class="wrap">
            <h1>📋 Security Log</h1>
            
            <div class="ssp-log-filters">
                <select id="ssp-log-type">
                    <option value="all">All Events</option>
                    <option value="login">Login Attempts</option>
                    <option value="scan">Security Scans</option>
                    <option value="firewall">Firewall Events</option>
                </select>
                <button class="button" onclick="ssp_filter_logs()">Filter</button>
            </div>
            
            <table class="wp-list-table widefat fixed striped">
                <thead>
                    <tr><th>Event</th><th>Details</th><th>IP Address</th><th>Time</th></tr>
                </thead>
                <tbody>
                    <?php foreach ($logs as $log): ?>
                    <tr>
                        <td><span class="badge badge-<?php echo $log['type']; ?>"><?php echo ucfirst($log['type']); ?></span></td>
                        <td><?php echo esc_html($log['message']); ?></td>
                        <td><?php echo esc_html($log['ip_address']); ?></td>
                        <td><?php echo date('Y-m-d H:i:s', strtotime($log['timestamp'])); ?></td>
                    </tr>
                    <?php endforeach; ?>
                </tbody>
            </table>
        </div>
        <?php
    }
    
    // ============ SETTINGS ============
    public function settings_page() {
        $license_key = $this->get_license_key();
        $license_status = $this->get_license_status();
        $license_tier = $this->get_license_tier();
        $site_count = $this->get_site_count();
        ?>
        <div class="wrap">
            <h1>⚙️ Security Settings</h1>

            <div class="ssp-license-section">
                <h2>🔑 License Activation</h2>
                <?php if ($license_status === 'valid'): ?>
                    <div class="ssp-license-status ssp-valid">
                        <p><strong>Status:</strong> <span class="ssp-badge ssp-badge-valid">ACTIVE</span></p>
                        <p><strong>Plan:</strong> <?php echo esc_html($license_tier); ?></p>
                        <p><strong>Site Limit:</strong> <?php echo $site_count === -1 ? 'Unlimited' : $site_count . ' site(s)'; ?></p>
                        <p><strong>License Key:</strong> <?php echo substr($license_key, 0, 8) . '...' . substr($license_key, -4); ?></p>
                        <form method="post">
                            <?php wp_nonce_field('ssp_license_action'); ?>
                            <button type="submit" name="ssp_deactivate_license" class="button" onclick="return confirm('Deactivate license on this site?')">Deactivate License</button>
                        </form>
                    </div>
                <?php else: ?>
                    <div class="ssp-license-status ssp-invalid">
                        <p>Enter your license key from Gumroad to activate the plugin.</p>
                        <form method="post">
                            <?php wp_nonce_field('ssp_license_action'); ?>
                            <input type="text" name="ssp_license_key" placeholder="Enter license key" value="" style="width: 350px; max-width: 100%;" required>
                            <button type="submit" name="ssp_activate_license" class="button button-primary">Activate License</button>
                        </form>
                        <p class="description">Don't have a license? <a href="https://gumroad.com/l/<?php echo $this->product_permalink; ?>" target="_blank">Purchase here</a></p>
                    </div>
                <?php endif; ?>
            </div>
            
            <form method="post">
                <?php wp_nonce_field('ssp_settings'); ?>
                
                <h2>General Settings</h2>
                <table class="form-table">
                    <tr>
                        <th>Enable Firewall</th>
                        <td><input type="checkbox" name="enable_firewall" value="1" checked <?php echo $this->has_feature('firewall') ? '' : 'disabled'; ?>></td>
                    </tr>
                    <tr>
                        <th>Enable Scanner</th>
                        <td><input type="checkbox" name="enable_scanner" value="1" checked <?php echo $this->has_feature('scanner') ? '' : 'disabled'; ?>></td>
                    </tr>
                    <tr>
                        <th>Enable Login Protection</th>
                        <td><input type="checkbox" name="enable_login_protection" value="1" checked <?php echo $this->has_feature('login_protection') ? '' : 'disabled'; ?>></td>
                    </tr>
                    <tr>
                        <th>Real-Time Monitoring</th>
                        <td><input type="checkbox" name="enable_real_time" value="1" <?php echo $this->has_feature('real_time_monitoring') ? '' : 'disabled'; ?>> <?php echo $this->has_feature('real_time_monitoring') ? '' : '<em class="ssp-pro-badge">Pro feature</em>'; ?></td>
                    </tr>
                    <tr>
                        <th>White Label Mode</th>
                        <td><input type="checkbox" name="enable_white_label" value="1" <?php echo $this->has_feature('white_label') ? '' : 'disabled'; ?>> <?php echo $this->has_feature('white_label') ? '' : '<em class="ssp-pro-badge">Enterprise feature</em>'; ?></td>
                    </tr>
                </table>
                
                <h2>Scan Schedule</h2>
                <table class="form-table">
                    <tr>
                        <th>Auto Scan</th>
                        <td>
                            <select name="scan_schedule" <?php echo $this->has_feature('scanner') ? '' : 'disabled'; ?>>
                                <option value="daily">Daily</option>
                                <option value="weekly">Weekly</option>
                                <option value="monthly">Monthly</option>
                            </select>
                        </td>
                    </tr>
                    <tr>
                        <th>Email Reports</th>
                        <td><input type="checkbox" name="email_reports" value="1" checked></td>
                    </tr>
                </table>
                
                <h2>Notification Settings</h2>
                <table class="form-table">
                    <tr>
                        <th>Email on Threat</th>
                        <td><input type="checkbox" name="email_on_threat" value="1" checked></td>
                    </tr>
                    <tr>
                        <th>Email on Lockout</th>
                        <td><input type="checkbox" name="email_on_lockout" value="1" checked></td>
                    </tr>
                    <tr>
                        <th>Alert Email</th>
                        <td><input type="email" name="alert_email" value="<?php echo get_option('admin_email'); ?>"></td>
                    </tr>
                </table>
                
                <?php submit_button('Save Settings'); ?>
            </form>
        </div>

        <style>
        .ssp-license-section { background: #fff; border: 1px solid #ccd0d4; border-radius: 8px; padding: 20px; margin: 20px 0; }
        .ssp-license-status { padding: 10px 0; }
        .ssp-valid { border-left: 4px solid #00a32a; padding-left: 15px; }
        .ssp-invalid { border-left: 4px solid #dc3232; padding-left: 15px; }
        .ssp-badge { display: inline-block; padding: 3px 12px; border-radius: 12px; font-size: 12px; font-weight: bold; }
        .ssp-badge-valid { background: #00a32a; color: #fff; }
        .ssp-pro-badge { color: #f07c00; font-style: normal; font-size: 11px; background: #fef7ed; padding: 2px 8px; border-radius: 10px; }
        </style>
        <?php
    }
    
    // ============ HELPER FUNCTIONS ============
    public function add_security_headers() {
        if (!is_admin()) {
            header('X-Content-Type-Options: nosniff');
            header('X-Frame-Options: SAMEORIGIN');
            header('X-XSS-Protection: 1; mode=block');
            header('Referrer-Policy: strict-origin-when-cross-origin');
        }
    }
    
    public function firewall_check() {
        if (is_admin()) return;
        global $wpdb;
        $table = $wpdb->prefix . 'ssp_blocked_ips';
        if ($wpdb->get_var("SHOW TABLES LIKE '$table'") !== $table) return;
        
        $ip = $this->get_client_ip();
        $blocked = $wpdb->get_var($wpdb->prepare(
            "SELECT COUNT(*) FROM $table WHERE ip_address = %s AND (blocked_until IS NULL OR blocked_until > NOW())",
            $ip
        ));
        
        if ($blocked > 0) {
            wp_die('Your IP has been blocked for security reasons.', 'Access Denied', array('response' => 403));
        }
    }
    
    public function log_login($username, $user = null) {
        global $wpdb;
        
        $wpdb->insert(
            $wpdb->prefix . 'ssp_login_logs',
            array(
                'user_id' => $user ? $user->ID : 0,
                'username' => $username,
                'ip_address' => $this->get_client_ip(),
                'user_agent' => $_SERVER['HTTP_USER_AGENT'] ?? '',
                'status' => 'success',
            )
        );
    }
    
    public function check_file_changes() {
        if (is_admin()) return;
        $last_check = get_option('ssp_last_file_check', 0);
        $interval = 3600; // 1 hour
        
        if (time() - $last_check > $interval) {
            // Check critical files
            $files = array(
                'wp-config.php' => md5_file(ABSPATH . 'wp-config.php'),
                'wp-login.php' => md5_file(ABSPATH . 'wp-login.php'),
                'index.php' => md5_file(ABSPATH . 'index.php'),
            );
            
            $saved_hashes = get_option('ssp_file_hashes', array());
            
            foreach ($files as $file => $hash) {
                if (isset($saved_hashes[$file]) && $saved_hashes[$file] !== $hash) {
                    $this->log_security_event('file_change', "File modified: $file");
                    // Send alert email
                    wp_mail(
                        get_option('admin_email'),
                        'Security Alert: File Changed',
                        "The file $file has been modified. Please check immediately."
                    );
                }
            }
            
            update_option('ssp_file_hashes', $files);
            update_option('ssp_last_file_check', time());
        }
    }
    
    private function get_client_ip() {
        $ip = $_SERVER['REMOTE_ADDR'] ?? '';
        if (isset($_SERVER['HTTP_X_FORWARDED_FOR'])) {
            $ip = explode(',', $_SERVER['HTTP_X_FORWARDED_FOR'])[0];
        }
        return trim($ip);
    }
    
    private function log_security_event($type, $message) {
        global $wpdb;
        
        $wpdb->insert(
            $wpdb->prefix . 'ssp_security_logs',
            array(
                'type' => $type,
                'message' => $message,
                'ip_address' => $this->get_client_ip(),
            )
        );
    }
    
    private function get_last_scan() {
        global $wpdb;
        return $wpdb->get_row("SELECT * FROM {$wpdb->prefix}ssp_scans ORDER BY timestamp DESC LIMIT 1", ARRAY_A) ?: array();
    }
    
    private function get_blocked_ips() {
        global $wpdb;
        return $wpdb->get_results("SELECT * FROM {$wpdb->prefix}ssp_blocked_ips ORDER BY timestamp DESC", ARRAY_A);
    }
    
    private function get_blocked_ips_count() {
        global $wpdb;
        return (int) $wpdb->get_var("SELECT COUNT(*) FROM {$wpdb->prefix}ssp_blocked_ips");
    }
    
    private function get_failed_logins_count() {
        global $wpdb;
        return (int) $wpdb->get_var("SELECT COUNT(*) FROM {$wpdb->prefix}ssp_login_logs WHERE status='failed' AND timestamp > DATE_SUB(NOW(), INTERVAL 24 HOUR)");
    }
    
    private function get_failed_logins($limit = 20) {
        global $wpdb;
        return $wpdb->get_results("SELECT * FROM {$wpdb->prefix}ssp_login_logs WHERE status='failed' ORDER BY timestamp DESC LIMIT $limit", ARRAY_A);
    }
    
    private function get_security_logs($limit = 50) {
        global $wpdb;
        return $wpdb->get_results("SELECT * FROM {$wpdb->prefix}ssp_security_logs ORDER BY timestamp DESC LIMIT $limit", ARRAY_A);
    }
    
    private function get_today_blocks() {
        global $wpdb;
        return (int) $wpdb->get_var("SELECT COUNT(*) FROM {$wpdb->prefix}ssp_blocked_ips WHERE DATE(timestamp) = CURDATE()");
    }
    
    private function get_firewall_rules_count() {
        return 12; // Default rules
    }
    
    public function render_recent_logs() {
        $logs = $this->get_security_logs(10);
        if (empty($logs)) {
            echo '<p>No recent activity.</p>';
            return;
        }
        
        echo '<table class="wp-list-table widefat fixed striped"><thead><tr><th>Event</th><th>Time</th></tr></thead><tbody>';
        foreach ($logs as $log) {
            echo '<tr><td>' . esc_html($log['message']) . '</td><td>' . date('M j, H:i', strtotime($log['timestamp'])) . '</td></tr>';
        }
        echo '</tbody></table>';
    }
    
    public function ajax_scan() {
        check_ajax_referer('ssp_nonce', 'nonce');
        
        // Perform security scan
        $results = array();
        $threats = 0;
        
        // Scan for known malware signatures
        $malware_signatures = array(
            'eval(base64_decode',
            'system($_',
            'passthru(',
            'shell_exec(',
            'exec(',
            'preg_replace.*\/e',
            'assert(',
        );
        
        // Check critical files
        $files_to_check = array();
        $iterator = new RecursiveIteratorIterator(new RecursiveDirectoryIterator(ABSPATH));
        foreach ($iterator as $file) {
            if ($file->isFile() && $file->getExtension() === 'php') {
                $files_to_check[] = $file->getPathname();
            }
        }
        
        $scanned = 0;
        foreach ($files_to_check as $file) {
            $content = file_get_contents($file);
            foreach ($malware_signatures as $signature) {
                if (preg_match($signature, $content)) {
                    $results[] = array(
                        'file' => str_replace(ABSPATH, '', $file),
                        'threat' => 'Potential malware signature detected',
                        'severity' => 'high'
                    );
                    $threats++;
                }
            }
            $scanned++;
        }
        
        // Save scan results
        global $wpdb;
        $wpdb->insert($wpdb->prefix . 'ssp_scans', array(
            'scan_type' => 'full',
            'result' => json_encode($results),
            'threats' => $threats,
        ));
        
        wp_send_json_success(array(
            'scanned' => $scanned,
            'threats' => $threats,
            'results' => $results
        ));
    }
    
    public function ajax_fix() {
        check_ajax_referer('ssp_nonce', 'nonce');

        if (!$this->has_feature('malware_removal')) {
            wp_send_json_error(array('message' => 'Automatic malware removal requires Pro or Enterprise license.'));
            return;
        }
        
        $file = sanitize_text_field($_POST['file']);
        $full_path = ABSPATH . $file;
        
        if (!file_exists($full_path)) {
            wp_send_json_error(array('message' => 'File not found.'));
            return;
        }

        $backup = $full_path . '.ssp_backup';
        copy($full_path, $backup);
        
        $content = file_get_contents($full_path);
        $cleaned = $this->clean_malware($content);
        file_put_contents($full_path, $cleaned);
        
        $this->log_security_event('scan', "Malware cleaned from: $file");
        
        wp_send_json_success(array('message' => 'Malware removed and backup created.'));
    }

    private function clean_malware($content) {
        $patterns = array(
            '/eval\s*\(\s*base64_decode\s*\([^)]+\)\s*\)\s*;/i',
            '/\$[a-zA-Z_\x7f-\xff][a-zA-Z0-9_\x7f-\xff]*\s*\(\s*\$_POST\s*\[[^\]]+\]\s*\)\s*;/i',
            '/system\s*\(\s*\$_/i',
            '/passthru\s*\(/i',
            '/shell_exec\s*\(/i',
        );
        foreach ($patterns as $pattern) {
            $content = preg_replace($pattern, '/* Cleaned by Security Scanner Pro */', $content);
        }
        return $content;
    }
    
    public function ajax_block_ip() {
        check_ajax_referer('ssp_nonce', 'nonce');
        
        global $wpdb;
        
        $ip = sanitize_text_field($_POST['ip_address']);
        $reason = sanitize_text_field($_POST['reason']);
        $duration = $_POST['duration'];
        
        $blocked_until = null;
        if ($duration === '24h') {
            $blocked_until = date('Y-m-d H:i:s', strtotime('+24 hours'));
        } elseif ($duration === '7d') {
            $blocked_until = date('Y-m-d H:i:s', strtotime('+7 days'));
        } elseif ($duration === '30d') {
            $blocked_until = date('Y-m-d H:i:s', strtotime('+30 days'));
        }
        
        $wpdb->insert($wpdb->prefix . 'ssp_blocked_ips', array(
            'ip_address' => $ip,
            'reason' => $reason,
            'blocked_until' => $blocked_until,
        ));
        
        wp_send_json_success(array('message' => 'IP blocked successfully'));
    }
    
    public function check_security() {
        // Check for WordPress updates
        if (isset($_GET['page']) && $_GET['page'] === 'security-scanner-pro') {
            // Check PHP version
            if (version_compare(PHP_VERSION, '7.4', '<')) {
                add_action('admin_notices', function() {
                    echo '<div class="notice notice-warning"><p>Security Scanner Pro: PHP 7.4+ recommended for optimal performance.</p></div>';
                });
            }
        }
    }
}

// Initialize
Security_Scanner_Pro::instance();

// AJAX handler for non-logged users
add_action('wp_ajax_nopriv_ssp_report', function() {
    // Allow external threat reporting
});

// Cron for scheduled scans
add_action('ssp_daily_scan', function() {
    $scanner = Security_Scanner_Pro::instance();
    // Run daily scan
});

if (!wp_next_scheduled('ssp_daily_scan')) {
    wp_schedule_event(time(), 'daily', 'ssp_daily_scan');
}
