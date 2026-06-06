// AutoCoupon Affiliate Module
// Gjeneron fitim përmes affiliate marketing

const AutoCouponAffiliate = {
  // Affiliate partners
  partners: {
    'amazon.com': {
      name: 'Amazon',
      affiliateId: 'autocoupon-20',
      commission: 0.05, // 5%
      url: 'https://www.amazon.com'
    },
    'ebay.com': {
      name: 'eBay',
      affiliateId: 'autocoupon-21',
      commission: 0.04, // 4%
      url: 'https://www.ebay.com'
    },
    'etsy.com': {
      name: 'Etsy',
      affiliateId: 'autocoupon-22',
      commission: 0.04, // 4%
      url: 'https://www.etsy.com'
    },
    'walmart.com': {
      name: 'Walmart',
      affiliateId: 'autocoupon-23',
      commission: 0.03, // 3%
      url: 'https://www.walmart.com'
    },
    'target.com': {
      name: 'Target',
      affiliateId: 'autocoupon-24',
      commission: 0.04, // 4%
      url: 'https://www.target.com'
    },
    'bestbuy.com': {
      name: 'Best Buy',
      affiliateId: 'autocoupon-25',
      commission: 0.03, // 3%
      url: 'https://www.bestbuy.com'
    },
    'nike.com': {
      name: 'Nike',
      affiliateId: 'autocoupon-26',
      commission: 0.05, // 5%
      url: 'https://www.nike.com'
    },
    'aliexpress.com': {
      name: 'AliExpress',
      affiliateId: 'autocoupon-27',
      commission: 0.06, // 6%
      url: 'https://www.aliexpress.com'
    }
  },

  // Track klikimet
  async trackClick(domain, couponCode) {
    try {
      const { affiliateClicks } = await chrome.storage.local.get('affiliateClicks');
      const clicks = affiliateClicks || [];
      
      clicks.push({
        domain: domain,
        couponCode: couponCode,
        timestamp: Date.now(),
        id: this.generateId()
      });

      // Mbaj vetëm 1000 klikimet e fundit
      if (clicks.length > 1000) {
        clicks.length = 1000;
      }

      await chrome.storage.local.set({ affiliateClicks: clicks });
      return { success: true };
    } catch (err) {
      console.error('Affiliate: Gabim gjatë tracking:', err);
      return { success: false, error: err.message };
    }
  },

  // Track konvertimin (blerjen)
  async trackConversion(domain, couponCode, purchaseAmount) {
    try {
      const partner = this.getPartner(domain);
      if (!partner) return { success: false, error: 'Partner nuk u gjet' };

      const commission = purchaseAmount * partner.commission;

      const { affiliateConversions } = await chrome.storage.local.get('affiliateConversions');
      const conversions = affiliateConversions || [];
      
      conversions.push({
        domain: domain,
        partner: partner.name,
        couponCode: couponCode,
        purchaseAmount: purchaseAmount,
        commission: commission,
        timestamp: Date.now(),
        id: this.generateId()
      });

      await chrome.storage.local.set({ affiliateConversions: conversions });
      
      // Përditëso të ardhurat totale
      await this.updateEarnings(commission);
      
      return { success: true, commission: commission };
    } catch (err) {
      console.error('Affiliate: Gabim gjatë tracking:', err);
      return { success: false, error: err.message };
    }
  },

  // Merr partnerin për domain
  getPartner(domain) {
    const cleanDomain = domain.replace(/^www\./, '').split('/')[0];
    
    for (const [key, partner] of Object.entries(this.partners)) {
      if (cleanDomain.includes(key) || key.includes(cleanDomain)) {
        return partner;
      }
    }
    return null;
  },

  // Krijo affiliate link
  createAffiliateLink(originalUrl, domain) {
    const partner = this.getPartner(domain);
    if (!partner) return originalUrl;

    // Shto affiliate ID në URL
    const url = new URL(originalUrl);
    
    switch (domain) {
      case 'amazon.com':
        url.searchParams.set('tag', partner.affiliateId);
        break;
      case 'ebay.com':
        url.searchParams.set('camp', '2093');
        url.searchParams.set('mkrid', '11850');
        url.searchParams.set('mktrid', '0');
        break;
      case 'etsy.com':
        url.searchParams.set('ref', partner.affiliateId);
        break;
      // Shto partnerë të tjerë sipas nevojës
    }

    return url.toString();
  },

  // Përditëso të ardhurat
  async updateEarnings(amount) {
    try {
      const { affiliateEarnings } = await chrome.storage.local.get('affiliateEarnings');
      const earnings = affiliateEarnings || {
        total: 0,
        thisMonth: 0,
        thisMonthStart: new Date().toISOString().slice(0, 7),
        history: []
      };

      earnings.total += amount;
      
      // Kontrollo nëse jemi në muajin e ri
      const currentMonth = new Date().toISOString().slice(0, 7);
      if (earnings.thisMonthStart !== currentMonth) {
        earnings.thisMonth = 0;
        earnings.thisMonthStart = currentMonth;
      }
      
      earnings.thisMonth += amount;
      
      // Shto në histori
      earnings.history.push({
        amount: amount,
        timestamp: Date.now()
      });

      // Mbaj vetëm 100 transaksionet e fundit
      if (earnings.history.length > 100) {
        earnings.history.length = 100;
      }

      await chrome.storage.local.set({ affiliateEarnings: earnings });
      return { success: true };
    } catch (err) {
      console.error('Affiliate: Gabim gjatë përditësimit:', err);
      return { success: false, error: err.message };
    }
  },

  // Merr të ardhurat
  async getEarnings() {
    try {
      const { affiliateEarnings } = await chrome.storage.local.get('affiliateEarnings');
      return affiliateEarnings || {
        total: 0,
        thisMonth: 0,
        thisMonthStart: new Date().toISOString().slice(0, 7),
        history: []
      };
    } catch (err) {
      return { total: 0, thisMonth: 0, history: [] };
    }
  },

  // Merr statistikat
  async getStats() {
    try {
      const { affiliateClicks } = await chrome.storage.local.get('affiliateClicks');
      const { affiliateConversions } = await chrome.storage.local.get('affiliateConversions');
      const earnings = await this.getEarnings();

      const clicks = affiliateClicks || [];
      const conversions = affiliateConversions || [];

      // Statistikat e këtij muaji
      const currentMonth = new Date().toISOString().slice(0, 7);
      const thisMonthClicks = clicks.filter(c => 
        new Date(c.timestamp).toISOString().slice(0, 7) === currentMonth
      );
      const thisMonthConversions = conversions.filter(c => 
        new Date(c.timestamp).toISOString().slice(0, 7) === currentMonth
      );

      return {
        totalClicks: clicks.length,
        thisMonthClicks: thisMonthClicks.length,
        totalConversions: conversions.length,
        thisMonthConversions: thisMonthConversions.length,
        conversionRate: clicks.length > 0 ? 
          ((conversions.length / clicks.length) * 100).toFixed(1) : '0.0',
        totalEarnings: earnings.total,
        thisMonthEarnings: earnings.thisMonth
      };
    } catch (err) {
      return {
        totalClicks: 0,
        thisMonthClicks: 0,
        totalConversions: 0,
        thisMonthConversions: 0,
        conversionRate: '0.0',
        totalEarnings: 0,
        thisMonthEarnings: 0
      };
    }
  },

  // Merr partnerët e disponueshëm
  getAvailablePartners() {
    return Object.entries(this.partners).map(([domain, partner]) => ({
      domain,
      name: partner.name,
      commission: (partner.commission * 100) + '%'
    }));
  },

  // Gjenero ID unike
  generateId() {
    return Date.now().toString(36) + Math.random().toString(36).slice(2, 8);
  }
};

// Eksporto nëse është modul
if (typeof module !== 'undefined' && module.exports) {
  module.exports = AutoCouponAffiliate;
}
