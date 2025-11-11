import api from "../configs/axios";
import { API_ENDPOINTS } from "../constants/apiConstants";

/**
 * Payment Service
 * Handles all payment-related API calls
 * @module paymentService
 */
const paymentService = {
  // ==========================================================================
  // BASIC CRUD OPERATIONS
  // ==========================================================================

  /**
   * Get all payments
   * @param {Object} params - Query parameters
   * @returns {Promise<Array>} Payments list
   */
  getPayments: async (params = {}) => {
    const response = await api.get(API_ENDPOINTS.PAYMENT.BASE, { params });
    return response.data;
  },

  /**
   * Create new payment
   * @param {Object} paymentData - Payment data
   * @param {number} paymentData.buyerId - Buyer member ID
   * @param {number} paymentData.sellerId - Seller member ID
   * @param {number} paymentData.amount - Payment amount
   * @param {string} paymentData.method - Payment method
   * @param {string} paymentData.transferContent - Transfer content/reference
   * @returns {Promise<Object>} Created payment
   */
  createPayment: async (paymentData) => {
    const response = await api.post(API_ENDPOINTS.PAYMENT.BASE, paymentData);
    return response.data;
  },

  /**
   * Get payment by ID
   * @param {number} id - Payment ID
   * @returns {Promise<Object>} Payment details
   */
  getPaymentById: async (id) => {
    const response = await api.get(API_ENDPOINTS.PAYMENT.BY_ID(id));
    return response.data;
  },

  /**
   * Update payment
   * @param {number} id - Payment ID
   * @param {Object} paymentData - Updated payment data
   * @returns {Promise<Object>} Updated payment
   */
  updatePayment: async (id, paymentData) => {
    const response = await api.put(API_ENDPOINTS.PAYMENT.BY_ID(id), paymentData);
    return response.data;
  },

  /**
   * Delete payment
   * @param {number} id - Payment ID
   * @returns {Promise<Object>} Delete response
   */
  deletePayment: async (id) => {
    const response = await api.delete(API_ENDPOINTS.PAYMENT.BY_ID(id));
    return response.data;
  },

  // ==========================================================================
  // PAYMENT QUERIES
  // ==========================================================================

  /**
   * Get payments by buyer
   * @param {number} buyerId - Buyer member ID
   * @returns {Promise<Array>} Buyer's payments
   */
  getPaymentsByBuyer: async (buyerId) => {
    const response = await api.get(API_ENDPOINTS.PAYMENT.BY_BUYER(buyerId));
    return response.data;
  },

  /**
   * Get payments by seller
   * @param {number} sellerId - Seller member ID
   * @returns {Promise<Array>} Seller's payments
   */
  getPaymentsBySeller: async (sellerId) => {
    const response = await api.get(API_ENDPOINTS.PAYMENT.BY_SELLER(sellerId));
    return response.data;
  },

  /**
   * Get payments by status
   * @param {string} status - Payment status (Pending/Completed/Failed/Cancelled/Refunded)
   * @returns {Promise<Array>} Filtered payments
   */
  getPaymentsByStatus: async (status) => {
    const response = await api.get(API_ENDPOINTS.PAYMENT.BY_STATUS(status));
    return response.data;
  },

  /**
   * Get payment statistics (admin/staff only)
   * @returns {Promise<Object>} Payment statistics
   */
  getPaymentStatistics: async () => {
    const response = await api.get(API_ENDPOINTS.PAYMENT.STATISTICS);
    return response.data;
  },

  // ==========================================================================
  // PAYMENT OPERATIONS
  // ==========================================================================

  /**
   * Update payment status
   * @param {number} id - Payment ID
   * @param {Object} statusData - Status data
   * @param {string} statusData.status - New status
   * @returns {Promise<Object>} Updated payment
   */
  updatePaymentStatus: async (id, statusData) => {
    const response = await api.put(API_ENDPOINTS.PAYMENT.UPDATE_STATUS(id), statusData);
    return response.data;
  },

  /**
   * Process payment
   * @param {number} id - Payment ID
   * @returns {Promise<Object>} Processing response
   */
  processPaymentById: async (id) => {
    const response = await api.post(API_ENDPOINTS.PAYMENT.PROCESS(id));
    return response.data;
  },

  // ==========================================================================
  // CHECKOUT / PAYMENT GATEWAY
  // ==========================================================================

  /**
   * Create checkout session (Stripe/Payment gateway)
   * @param {Object} checkoutData - Checkout data
   * @param {number} checkoutData.postId - Post ID
   * @param {number} checkoutData.packageId - Package ID
   * @returns {Promise<Object>} Checkout response with URL
   */
  createCheckout: async (checkoutData) => {
    const response = await api.post(API_ENDPOINTS.PAYMENT.CHECKOUT, checkoutData);
    return response.data;
  },

  /**
   * Open checkout URL in new window
   * @param {string} checkoutUrl - Checkout URL
   */
  openCheckout: (checkoutUrl) => {
    if (!checkoutUrl) {
      throw new Error("Missing checkout URL");
    }
    window.open(checkoutUrl, "_blank");
  },

  /**
   * Handle payment webhook (internal use)
   * This is typically called by payment gateway, not frontend
   * @param {Object} webhookData - Webhook data
   * @returns {Promise<Object>} Webhook response
   */
  handleWebhook: async (webhookData) => {
    const response = await api.post(API_ENDPOINTS.PAYMENT.WEBHOOK, webhookData);
    return response.data;
  },

  /**
   * Test webhook endpoint (development only)
   * @param {Object} testData - Test webhook data
   * @returns {Promise<Object>} Test response
   */
  testWebhook: async (testData) => {
    const response = await api.post(API_ENDPOINTS.PAYMENT.WEBHOOK_TEST, testData);
    return response.data;
  },
};

export default paymentService;
