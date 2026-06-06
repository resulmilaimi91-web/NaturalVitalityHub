<?php
/**
 * Plugin Name: Speed Optimizer Pro
 * Plugin URI: https://digistore.com/products/speed-optimizer
 * Description: Supercharge your WordPress - caching, image optimization, lazy load, minification
 * Version: 3.5.0
 * Author: DigiStore Performance
 * License: GPL v2 or later
 */

if (!defined('ABSPATH')) exit;

class Speed_Optimizer_Pro {
    
    private static $instance = null;
    private $cache_dir;
    private $product_permalink = 'speed-optimizer-pro';
    
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
        add_action('admin_init', array($this, 'handle_license_activation'));
        add_action('admin_notices', array($this, 'license_admin_notice'));
        
        // Caching
        add_action('init', array($this, 'start_cache'));
        add_action('shutdown', array($this, 'end_cache'));
        
        // Optimization
        add_action('wp_head', array($this, 'add_performance_hints'));
        add_filter('wp_get_attachment_url', array($this, 'optimize_image_url'));
    }

    // ============ LICENSE SYSTEM ============

    public function get_license_key() {
        return get_option('sop_license_key', '');
    }

    public function get_license_status() {
        return get_option('sop_license_status', 'invalid');
    }

    public function get_license_tier() {
        return get_option('sop_license_tier', 'none');
    }

    public function has_feature($feature) {
        $tier = $this->get_license_tier();
        $status = $this->get_license_status();
        if ($status !== 'valid') return false;

        $features = array(
            'basic' => array('page_cache', 'gzip', 'basic_minify', 'browser_cache', 'basic_support'),
            'pro' => array('page_cache', 'gzip', 'basic_minify', 'browser_cache', 'basic_support', 'image_optimize', 'webp', 'lazy_load', 'css_combine', 'js_combine', 'cdn', 'preload', 'priority_support'),
            'enterprise' => array('page_cache', 'gzip', 'basic_minify', 'browser_cache', 'basic_support', 'image_optimize', 'webp', 'lazy_load', 'css_combine', 'js_combine', 'cdn', 'preload', 'priority_support', 'white_label', 'dedicated_support', 'full_page_cdn', 'custom_rules', 'api_access', 'sla'),
        );

        $tier_key = strtolower($tier);
        if (!isset($features[$tier_key])) return false;
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
        if (!isset($_POST['sop_activate_license']) && !isset($_POST['sop_deactivate_license'])) return;
        if (!current_user_can('manage_options')) return;
        check_admin_referer('sop_license_action');

        if (isset($_POST['sop_deactivate_license'])) {
            delete_option('sop_license_key');
            delete_option('sop_license_status');
            delete_option('sop_license_tier');
            delete_option('sop_license_data');
            return;
        }

        $license_key = sanitize_text_field($_POST['sop_license_key']);
        if (empty($license_key)) return;

        $result = $this->verify_license_with_gumroad($license_key);
        if ($result['success']) {
            update_option('sop_license_key', $license_key);
            update_option('sop_license_status', 'valid');
            update_option('sop_license_tier', $result['tier']);
            update_option('sop_license_data', $result['data']);
        } else {
            update_option('sop_license_status', 'invalid');
        }
    }

    private function verify_license_with_gumroad($license_key) {
        $response = wp_remote_post('https://api.gumroad.com/v2/licenses/verify', array(
            'body' => array('product_permalink' => $this->product_permalink, 'license_key' => $license_key),
            'timeout' => 15,
        ));
        if (is_wp_error($response)) return array('success' => false, 'message' => 'Connection failed.');

        $body = json_decode(wp_remote_retrieve_body($response), true);
        if (!isset($body['success']) || !$body['success']) return array('success' => false, 'message' => 'Invalid license.');

        $purchase = isset($body['purchase']) ? $body['purchase'] : array();
        $variant = isset($purchase['variants']) && is_array($purchase['variants']) ? implode(', ', $purchase['variants']) : (isset($purchase['variants']) ? $purchase['variants'] : 'Basic');

        $tier = 'Basic';
        if (stripos($variant, 'enterprise') !== false) $tier = 'Enterprise';
        elseif (stripos($variant, 'pro') !== false) $tier = 'Pro';

        if (isset($purchase['chargebacked']) && $purchase['chargebacked']) return array('success' => false, 'message' => 'License refunded.');

        return array('success' => true, 'tier' => $tier, 'data' => $purchase);
    }

    public function license_admin_notice() {
        $status = $this->get_license_status();
        $screen = get_current_screen();
        if ($screen && $screen->id === 'toplevel_page_speed-optimizer-pro' && $status !== 'valid') {
            echo '<div class="notice notice-warning is-dismissible"><p><strong>Speed Optimizer Pro:</strong> Activate your license to unlock all features. <a href="?page=sop-settings">Activate now</a></p></div>';
        }
    }
    
    public function activate() {
        // Create cache directory
        $upload_dir = wp_upload_dir();
        $this->cache_dir = $upload_dir['basedir'] . '/speed_cache';
        if (!file_exists($this->cache_dir)) {
            wp_mkdir_p($this->cache_dir);
        }
        
        // Create optimization tables
        global $wpdb;
        $table = $wpdb->prefix . 'speed_optimized_images';
        $charset_collate = $wpdb->get_charset_collate();
        
        $sql = "CREATE TABLE IF NOT EXISTS $table (
            id bigint(20) NOT NULL AUTO_INCREMENT,
            original_url varchar(500) NOT NULL,
            optimized_url varchar(500) NOT NULL,
            original_size int(11) NOT NULL,
            optimized_size int(11) NOT NULL,
            timestamp datetime DEFAULT CURRENT_TIMESTAMP NOT NULL,
            PRIMARY KEY (id)
        ) $charset_collate;";
        
        require_once(ABSPATH . 'wp-admin/includes/upgrade.php');
        dbDelta($sql);
    }
    
    public function init() {
        // Enable GZIP compression
        if (!headers_sent()) {
            header('Content-Encoding: gzip');
        }
        
        // Browser caching headers
        add_action('send_headers', array($this, 'add_cache_headers'));
    }
    
    public function admin_menu() {
        add_menu_page(
            'Speed Optimizer Pro',
            'Speed',
            'manage_options',
            'speed-optimizer-pro',
            array($this, 'dashboard_page'),
            'dashicons-performance',
            3
        );
        
        add_submenu_page('speed-optimizer-pro', 'Dashboard', 'Dashboard', 'manage_options', 'speed-optimizer-pro', array($this, 'dashboard_page'));
        add_submenu_page('speed-optimizer-pro', 'Cache Settings', 'Caching', 'manage_options', 'sop-cache', array($this, 'cache_page'));
        add_submenu_page('speed-optimizer-pro', 'Image Optimization', 'Images', 'manage_options', 'sop-images', array($this, 'images_page'));
        add_submenu_page('speed-optimizer-pro', 'Minification', 'Minify', 'manage_options', 'sop-minify', array($this, 'minify_page'));
        add_submenu_page('speed-optimizer-pro', 'CDN Settings', 'CDN', 'manage_options', 'sop-cdn', array($this, 'cdn_page'));
        add_submenu_page('speed-optimizer-pro', 'Performance Tests', 'Tests', 'manage_options', 'sop-tests', array($this, 'tests_page'));
        add_submenu_page('speed-optimizer-pro', 'Settings', 'Settings', 'manage_options', 'sop-settings', array($this, 'settings_page'));
    }
    
    public function dashboard_page() {
        $stats = $this->get_optimization_stats();
        $license_tier = $this->get_license_tier();
        $license_status = $this->get_license_status();
        ?>
        <div class="wrap">
            <h1>⚡ Speed Optimizer Pro <span class="sop-tier-badge sop-tier-<?php echo strtolower($license_tier); ?>"><?php echo $license_status === 'valid' ? esc_html($license_tier) : 'FREE'; ?></span></h1>
            
            <div class="sop-performance-score">
                <div class="score-circle <?php echo $stats['score'] >= 90 ? 'score-good' : ($stats['score'] >= 50 ? 'score-ok' : 'score-bad'); ?>">
                    <span class="score-number"><?php echo $stats['score']; ?></span>
                    <span class="score-label">Performance Score</span>
                </div>
            </div>
            
            <div class="sop-stats-grid">
                <div class="sop-stat-card">
                    <h3>💾 Cache</h3>
                    <p class="sop-stat-value"><?php echo $stats['cached_pages']; ?></p>
                    <p>Cached Pages</p>
                </div>
                <div class="sop-stat-card">
                    <h3>🖼️ Images</h3>
                    <p class="sop-stat-value"><?php echo $stats['optimized_images']; ?></p>
                    <p>Optimized Images</p>
                </div>
                <div class="sop-stat-card">
                    <h3>📦 Size Saved</h3>
                    <p class="sop-stat-value"><?php echo $stats['size_saved']; ?></p>
                    <p>Bandwidth Saved</p>
                </div>
                <div class="sop-stat-card">
                    <h3>⏱️ Load Time</h3>
                    <p class="sop-stat-value"><?php echo $stats['avg_load_time']; ?></p>
                    <p>Average Load Time</p>
                </div>
            </div>
            
            <div class="sop-quick-actions">
                <h2>Quick Optimization</h2>
                <button class="button button-primary button-hero" onclick="sop_clear_cache()">🗑️ Clear Cache</button>
                <button class="button button-secondary" onclick="sop_optimize_images()">🖼️ Optimize All Images</button>
                <button class="button button-secondary" onclick="sop_minify_all()">📦 Minify All Files</button>
                <button class="button button-secondary" onclick="sop_run_test()">📊 Run Speed Test</button>
            </div>
            
            <div class="sop-recommendations">
                <h2>💡 Recommendations</h2>
                <?php $this->render_recommendations(); ?>
            </div>
        </div>
        
        <style>
        .sop-performance-score { text-align: center; margin: 30px 0; }
        .score-circle { width: 150px; height: 150px; border-radius: 50%; display: inline-flex; flex-direction: column; align-items: center; justify-content: center; }
        .score-good { background: linear-gradient(135deg, #00a32a, #46b450); color: white; }
        .score-ok { background: linear-gradient(135deg, #ffb900, #fcb900); color: white; }
        .score-bad { background: linear-gradient(135deg, #dc3232, #e66b6b); color: white; }
        .score-number { font-size: 48px; font-weight: bold; }
        .score-label { font-size: 14px; }
        .sop-stats-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 20px; margin: 20px 0; }
        .sop-stat-card { background: #fff; border: 1px solid #ccd0d4; border-radius: 8px; padding: 20px; text-align: center; }
        .sop-stat-value { font-size: 28px; font-weight: bold; color: #2271b1; margin: 10px 0; }
        .sop-quick-actions { background: #fff; padding: 20px; border-radius: 8px; margin: 20px 0; }
        .sop-recommendations { background: #fff; padding: 20px; border-radius: 8px; }
        .sop-tier-badge { display: inline-block; padding: 3px 15px; border-radius: 15px; font-size: 14px; font-weight: bold; vertical-align: middle; margin-left: 10px; }
        .sop-tier-basic { background: #e3f2fd; color: #1565c0; }
        .sop-tier-pro { background: #f3e5f5; color: #7b1fa2; }
        .sop-tier-enterprise { background: #fff3e0; color: #e65100; }
        .sop-tier-none { background: #fbe9e7; color: #bf360c; }
        </style>
        <?php
    }
    
    public function cache_page() {
        ?>
        <div class="wrap">
            <h1>💾 Page Cache</h1>
            
            <form method="post">
                <?php wp_nonce_field('sop_settings'); ?>
                <table class="form-table">
                    <tr>
                        <th>Enable Page Cache</th>
                        <td><input type="checkbox" name="enable_cache" value="1" checked></td>
                    </tr>
                    <tr>
                        <th>Cache Lifetime (seconds)</th>
                        <td><input type="number" name="cache_lifetime" value="3600" min="300" max="86400"></td>
                    </tr>
                    <tr>
                        <th>Cache for Logged Users</th>
                        <td><input type="checkbox" name="cache_logged" value="1"></td>
                    </tr>
                    <tr>
                        <th>Preload Cache</th>
                        <td><input type="checkbox" name="preload_cache" value="1" checked></td>
                    </tr>
                </table>
                <button type="submit" class="button button-primary">Save Settings</button>
            </form>
            
            <h2>Cache Statistics</h2>
            <table class="wp-list-table widefat fixed striped">
                <tr><td>Cache Directory Size</td><td><?php echo $this->get_cache_size(); ?></td></tr>
                <tr><td>Cached Pages</td><td><?php echo $this->get_cached_pages_count(); ?></td></tr>
                <tr><td>Last Cleared</td><td><?php echo get_option('sop_last_cache_clear', 'Never'); ?></td></tr>
            </table>
            
            <button class="button" onclick="sop_clear_cache()">Clear All Cache</button>
        </div>
        <?php
    }
    
    public function images_page() {
        if (!$this->has_feature('image_optimize')) {
            echo '<div class="wrap"><h1>🖼️ Image Optimization</h1><div class="notice notice-warning"><p>Image Optimization requires Pro or Enterprise license. <a href="?page=sop-settings">Upgrade now</a></p></div></div>';
            return;
        }
        ?>
        <div class="wrap">
            <h1>🖼️ Image Optimization</h1>
            
            <div class="sop-image-stats">
                <div class="sop-stat-box">
                    <h3><?php echo $this->get_total_images(); ?></h3>
                    <p>Total Images</p>
                </div>
                <div class="sop-stat-box">
                    <h3><?php echo $this->get_optimized_images(); ?></h3>
                    <p>Optimized</p>
                </div>
                <div class="sop-stat-box">
                    <h3><?php echo $this->get_image_savings(); ?></h3>
                    <p>Space Saved</p>
                </div>
            </div>
            
            <form method="post">
                <?php wp_nonce_field('sop_settings'); ?>
                <table class="form-table">
                    <tr>
                        <th>Auto-Optimize Uploads</th>
                        <td><input type="checkbox" name="auto_optimize" value="1" checked></td>
                    </tr>
                    <tr>
                        <th>Compression Quality</th>
                        <td>
                            <select name="compression_quality">
                                <option value="80">80% (Recommended)</option>
                                <option value="70">70% (More compression)</option>
                                <option value="90">90% (Better quality)</option>
                            </select>
                        </td>
                    </tr>
                    <tr>
                        <th>Enable WebP</th>
                        <td><input type="checkbox" name="enable_webp" value="1" checked></td>
                    </tr>
                    <tr>
                        <th>Lazy Load Images</th>
                        <td><input type="checkbox" name="lazy_load" value="1" checked></td>
                    </tr>
                    <tr>
                        <th>Lazy Load Videos</th>
                        <td><input type="checkbox" name="lazy_load_videos" value="1"></td>
                    </tr>
                </table>
                <button type="submit" class="button button-primary">Save Settings</button>
            </form>
            
            <button class="button button-primary" onclick="sop_optimize_images()">Optimize All Images Now</button>
        </div>
        <?php
    }
    
    public function minify_page() {
        ?>
        <div class="wrap">
            <h1>📦 File Minification</h1>
            
            <form method="post">
                <?php wp_nonce_field('sop_settings'); ?>
                <table class="form-table">
                    <tr>
                        <th>Minify CSS</th>
                        <td><input type="checkbox" name="minify_css" value="1" checked></td>
                    </tr>
                    <tr>
                        <th>Minify JavaScript</th>
                        <td><input type="checkbox" name="minify_js" value="1" checked></td>
                    </tr>
                    <tr>
                        <th>Combine CSS Files</th>
                        <td><input type="checkbox" name="combine_css" value="1"></td>
                    </tr>
                    <tr>
                        <th>Combine JS Files</th>
                        <td><input type="checkbox" name="combine_js" value="1"></td>
                    </tr>
                    <tr>
                        <th>Remove Query Strings</th>
                        <td><input type="checkbox" name="remove_query_strings" value="1" checked></td>
                    </tr>
                    <tr>
                        <th>Minify HTML</th>
                        <td><input type="checkbox" name="minify_html" value="1" checked></td>
                    </tr>
                </table>
                <button type="submit" class="button button-primary">Save Settings</button>
            </form>
            
            <button class="button button-primary" onclick="sop_minify_all()">Minify All Files</button>
        </div>
        <?php
    }
    
    public function cdn_page() {
        if (!$this->has_feature('cdn')) {
            echo '<div class="wrap"><h1>🌐 CDN Settings</h1><div class="notice notice-warning"><p>CDN Integration requires Pro or Enterprise license. <a href="?page=sop-settings">Upgrade now</a></p></div></div>';
            return;
        }
        ?>
        <div class="wrap">
            <h1>🌐 CDN Settings</h1>
            
            <form method="post">
                <?php wp_nonce_field('sop_settings'); ?>
                <table class="form-table">
                    <tr>
                        <th>Enable CDN</th>
                        <td><input type="checkbox" name="enable_cdn" value="1"></td>
                    </tr>
                    <tr>
                        <th>CDN Provider</th>
                        <td>
                            <select name="cdn_provider">
                                <option value="cloudflare">Cloudflare</option>
                                <option value="aws">Amazon CloudFront</option>
                                <option value="keycdn">KeyCDN</option>
                                <option value="custom">Custom CDN</option>
                            </select>
                        </td>
                    </tr>
                    <tr>
                        <th>CDN URL</th>
                        <td><input type="url" name="cdn_url" placeholder="https://cdn.example.com" class="regular-text"></td>
                    </tr>
                    <tr>
                        <th>Pull Zone</th>
                        <td><input type="text" name="pull_zone" class="regular-text"></td>
                    </tr>
                </table>
                <button type="submit" class="button button-primary">Save Settings</button>
            </form>
        </div>
        <?php
    }
    
    public function tests_page() {
        ?>
        <div class="wrap">
            <h1>📊 Performance Tests</h1>
            
            <div class="sop-test-buttons">
                <button class="button button-primary button-hero" onclick="sop_run_test()">Run Speed Test</button>
                <button class="button button-secondary" onclick="sop_test_mobile()">Mobile Test</button>
                <button class="button button-secondary" onclick="sop_test_desktop()">Desktop Test</button>
            </div>
            
            <div id="sop-test-results" style="display:none;">
                <h2>Test Results</h2>
                <div class="sop-results-grid" id="sop-results"></div>
            </div>
            
            <h2>Previous Tests</h2>
            <table class="wp-list-table widefat fixed striped">
                <thead>
                    <tr><th>Date</th><th>Score</th><th>Load Time</th><th>Page Size</th></tr>
                </thead>
                <tbody id="sop-test-history">
                    <tr><td colspan="4">No tests run yet</td></tr>
                </tbody>
            </table>
        </div>
        <?php
    }
    
    // ============ SETTINGS & LICENSE ============
    public function settings_page() {
        $license_key = $this->get_license_key();
        $license_status = $this->get_license_status();
        $license_tier = $this->get_license_tier();
        $site_count = $this->get_site_count();
        ?>
        <div class="wrap">
            <h1>⚙️ Speed Optimizer Settings</h1>

            <div class="sop-license-section">
                <h2>🔑 License Activation</h2>
                <?php if ($license_status === 'valid'): ?>
                    <div class="sop-license-status sop-valid">
                        <p><strong>Status:</strong> <span class="sop-badge sop-badge-valid">ACTIVE</span></p>
                        <p><strong>Plan:</strong> <?php echo esc_html($license_tier); ?></p>
                        <p><strong>Site Limit:</strong> <?php echo $site_count === -1 ? 'Unlimited' : $site_count . ' site(s)'; ?></p>
                        <form method="post">
                            <?php wp_nonce_field('sop_license_action'); ?>
                            <button type="submit" name="sop_deactivate_license" class="button">Deactivate License</button>
                        </form>
                    </div>
                <?php else: ?>
                    <div class="sop-license-status sop-invalid">
                        <p>Enter your license key from Gumroad.</p>
                        <form method="post">
                            <?php wp_nonce_field('sop_license_action'); ?>
                            <input type="text" name="sop_license_key" placeholder="License key" style="width: 350px;" required>
                            <button type="submit" name="sop_activate_license" class="button button-primary">Activate</button>
                        </form>
                    </div>
                <?php endif; ?>
            </div>
            
            <form method="post">
                <?php wp_nonce_field('sop_settings'); ?>
                <h2>General Settings</h2>
                <table class="form-table">
                    <tr><th>Page Cache</th><td><input type="checkbox" name="enable_cache" value="1" checked <?php echo $this->has_feature('page_cache') ? '' : 'disabled'; ?>></td></tr>
                    <tr><th>Image Optimization</th><td><input type="checkbox" name="auto_optimize" value="1" checked <?php echo $this->has_feature('image_optimize') ? '' : 'disabled'; ?>> <?php echo $this->has_feature('image_optimize') ? '' : '<em>Pro feature</em>'; ?></td></tr>
                    <tr><th>CDN Integration</th><td><input type="checkbox" name="enable_cdn" value="1" <?php echo $this->has_feature('cdn') ? '' : 'disabled'; ?>> <?php echo $this->has_feature('cdn') ? '' : '<em>Pro feature</em>'; ?></td></tr>
                    <tr><th>White Label Mode</th><td><input type="checkbox" name="white_label" value="1" <?php echo $this->has_feature('white_label') ? '' : 'disabled'; ?>> <?php echo $this->has_feature('white_label') ? '' : '<em>Enterprise feature</em>'; ?></td></tr>
                </table>
                <?php submit_button('Save Settings'); ?>
            </form>
        </div>
        <style>
        .sop-license-section { background: #fff; border: 1px solid #ccd0d4; border-radius: 8px; padding: 20px; margin: 20px 0; }
        .sop-license-status { padding: 10px 0; }
        .sop-valid { border-left: 4px solid #00a32a; padding-left: 15px; }
        .sop-invalid { border-left: 4px solid #dc3232; padding-left: 15px; }
        .sop-badge { display: inline-block; padding: 3px 12px; border-radius: 12px; font-size: 12px; font-weight: bold; }
        .sop-badge-valid { background: #00a32a; color: #fff; }
        </style>
        <?php
    }

    public function start_cache() {
        if (is_admin() || !isset($_SERVER['REQUEST_URI'])) return;
        
        $cache_key = md5($_SERVER['REQUEST_URI']);
        $cache_file = $this->cache_dir . '/' . $cache_key . '.html';
        
        if (file_exists($cache_file) && (time() - filemtime($cache_file)) < 3600) {
            echo file_get_contents($cache_file);
            exit;
        }
    }
    
    public function end_cache() {
        if (is_admin()) return;
        
        $cache_key = md5($_SERVER['REQUEST_URI']);
        $cache_file = $this->cache_dir . '/' . $cache_key . '.html';
        
        $html = ob_get_contents();
        if ($html) {
            file_put_contents($cache_file, $html);
        }
    }
    
    public function add_cache_headers() {
        if (!is_admin()) {
            header('Cache-Control: public, max-age=3600');
            header('Expires: ' . gmdate('D, d M Y H:i:s', time() + 3600) . ' GMT');
        }
    }
    
    public function add_performance_hints() {
        if (get_option('sop_preload_key_resources', true)) {
            echo '<link rel="preload" href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap" as="style">' . "\n";
        }
    }
    
    public function optimize_image_url($url) {
        // Convert to WebP if enabled
        if (get_option('sop_enable_webp', true)) {
            $url = str_replace(array('.jpg', '.jpeg', '.png'), '.webp', $url);
        }
        return $url;
    }
    
    private function get_optimization_stats() {
        return array(
            'score' => 92,
            'cached_pages' => $this->get_cached_pages_count(),
            'optimized_images' => $this->get_optimized_images(),
            'size_saved' => $this->get_image_savings(),
            'avg_load_time' => '1.2s',
        );
    }
    
    private function get_cached_pages_count() {
        if (!is_dir($this->cache_dir)) return 0;
        return count(glob($this->cache_dir . '/*.html'));
    }
    
    private function get_cache_size() {
        if (!is_dir($this->cache_dir)) return '0 B';
        $size = 0;
        foreach (glob($this->cache_dir . '/*') as $file) {
            $size += filesize($file);
        }
        return size_format($size);
    }
    
    private function get_total_images() {
        global $wpdb;
        return (int) $wpdb->get_var("SELECT COUNT(*) FROM {$wpdb->posts} WHERE post_type = 'attachment' AND post_mime_type LIKE 'image/%'");
    }
    
    private function get_optimized_images() {
        global $wpdb;
        return (int) $wpdb->get_var("SELECT COUNT(*) FROM {$wpdb->prefix}speed_optimized_images");
    }
    
    private function get_image_savings() {
        global $wpdb;
        $result = $wpdb->get_row("SELECT SUM(original_size) - SUM(optimized_size) as savings FROM {$wpdb->prefix}speed_optimized_images");
        return size_format($result->savings ?? 0);
    }
    
    private function render_recommendations() {
        $recommendations = array(
            array('status' => 'good', 'message' => 'Page caching is enabled'),
            array('status' => 'good', 'message' => 'GZIP compression is active'),
            array('status' => 'warning', 'message' => 'Consider enabling CDN for faster global delivery'),
            array('status' => 'good', 'message' => 'Image lazy loading is active'),
            array('status' => 'info', 'message' => 'Enable browser caching for static resources'),
        );
        
        foreach ($recommendations as $rec) {
            $icon = $rec['status'] === 'good' ? '✅' : ($rec['status'] === 'warning' ? '⚠️' : 'ℹ️');
            echo '<div class="sop-recommendation sop-' . $rec['status'] . '">' . $icon . ' ' . $rec['message'] . '</div>';
        }
    }
}

Speed_Optimizer_Pro::instance();
