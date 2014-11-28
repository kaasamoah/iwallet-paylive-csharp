using System;
using System.Collections.Generic;
using System.Text;
using iWalletPayliveModule.Paylive;
using System.Configuration;

namespace iWalletPayliveModule
{
    /// <summary>
    /// Helper class for interacting with the iWallet PayLIVE! service
    /// </summary>
    public class PayliveConnector
    {
        #region Fields
        private String _apiVersion;
        private String _merchantEmail;
        private String _merchantKey;
        private String _serviceType;
        private bool _integrationMode;
        #endregion

        #region Properties
        /// <summary>
        /// The version of the PayLIVE! API in use
        /// </summary>
        public String ApiVersion
        {
            get { return _apiVersion; }
            set { _apiVersion = value; }
        }

        /// <summary>
        /// Merchant's registered iWallet email
        /// </summary>
        public String MerchantEmail
        {
            get { return _merchantEmail; }
            set { _merchantEmail = value; }
        }

        /// <summary>
        /// Merchant's API key
        /// </summary>
        public String MerchantKey
        {
            get { return _merchantKey; }
            set { _merchantKey = value; }
        }

        /// <summary>
        /// Payment service type (defaults to C2B)
        /// </summary>
        public String ServiceType
        {
            get { return _serviceType; }
            set { _serviceType = value; }
        }

        /// <summary>
        /// Flag indicating whether or not merchant is in integration mode
        /// </summary>
        public bool IntegrationMode
        {
            get { return _integrationMode; }
            set { _integrationMode = value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of PayliveConnector. Payment header values will be read from configuration file if available
        /// </summary>
        public PayliveConnector()
        {
            _apiVersion = ConfigurationManager.AppSettings["apiVersion"];
            _merchantEmail = ConfigurationManager.AppSettings["merchantEmail"];
            _merchantKey = ConfigurationManager.AppSettings["merchantKey"];
            _serviceType = ConfigurationManager.AppSettings["serviceType"];
            _integrationMode = Convert.ToBoolean(ConfigurationManager.AppSettings["integrationMode"]);
        }

        /// <summary>
        /// Creates a new instance of PayliveConnector with Payment header values set to the parameters provided
        /// </summary>
        public PayliveConnector(string apiVersion, string merchantEmail, string merchantKey, string serviceType, bool integrationMode)
        {
            _apiVersion = apiVersion;
            _merchantEmail = merchantEmail;
            _merchantKey = merchantKey;
            _serviceType = serviceType;
            _integrationMode = integrationMode;
        }
        #endregion

        #region Paylive Methods
        /// <summary>
        /// Generates a payment order and returns the payment token as well as a QR code URL and payment order code for iWallet Cruize payments
        /// </summary>
        /// <param name="orderId">Merchant's order id. Unique code identifying the order on merchant's system</param>
        /// <param name="subTotal">Total cost of items</param>
        /// <param name="deliveryCost">Cost of delivery</param>
        /// <param name="taxAmount">Amount going to tax</param>
        /// <param name="total">Total amount payable by customer</param>
        /// <param name="comment1">A short description of order</param>
        /// <param name="comment2">Any extra information about order</param>
        /// <param name="items">List of items being purchased</param>
        /// <returns>A mobile payment order response containing a payment token, QR code URL and payment order code</returns>
        public MobilePaymentOrderResponse mobilePayment(string orderId, decimal subTotal, decimal deliveryCost, decimal taxAmount, decimal total, string comment1, string comment2, OrderItem[] items)
        {
            PaymentService service = Initialize();
            MobilePaymentOrderResponse result = service.mobilePaymentOrder(orderId, subTotal, true, deliveryCost, true, taxAmount, true, total, true, comment1, comment2, items);
            return result;
        }

        /// <summary>
        /// Checks the status of a mobile payment
        /// </summary>
        /// <param name="orderId">Merchant's order id. Unique code identifying the order on merchant's system</param>
        /// <returns>An object containing the details of the payment including the status and transaction id</returns>
        public VerifyMobilePaymentResponse verifyMobilePaymentStatus(String orderId)
        {
            PaymentService service = Initialize();
            VerifyMobilePaymentResponse result = service.verifyMobilePayment(orderId);
            return result;
        }

        /// <summary>
        /// Generates a payment order code to be used when paying via third party payment providers
        /// </summary>
        /// <param name="orderId">Merchant's order id. Unique code identifying the order on merchant's system</param>
        /// <param name="subTotal">Total cost of items</param>
        /// <param name="deliveryCost">Cost of delivery</param>
        /// <param name="taxAmount">Amount going to tax</param>
        /// <param name="total">Total amount payable by customer</param>
        /// <param name="comment1">A short description of order</param>
        /// <param name="comment2">Any extra information about order</param>
        /// <param name="items">List of items being purchased</param>
        /// <param name="payer">Name of payer</param>
        /// <param name="mobile">Mobile number of payer. Required for mobile money providers</param>
        /// <param name="provider">The third party payment provider</param>
        /// <param name="providerType">Type of provider (eg. MOBILE_MONEY, BANK, CARD, etc)</param>
        /// <returns>The generated payment code</returns>
        public String getPaymentCode(string orderId, decimal subTotal, decimal deliveryCost, decimal taxAmount, decimal total, string comment1, string comment2, OrderItem[] items, String payer, String mobile, String provider, String providerType)
        {
            PaymentService service = Initialize();
            string code = service.generatePaymentCode(orderId, subTotal, true, deliveryCost, true, taxAmount, true, total, true, comment1, comment2, items, payer, mobile, provider, providerType);
            return code;
        }

        /// <summary>
        /// Checks the status of a third party payment
        /// </summary>
        /// <param name="orderId">Merchant's order id. Unique code identifying the order on merchant's system</param>
        /// <param name="provider">The third party payment provider</param>
        /// <param name="providerType">Type of provider (eg. MOBILE_MONEY, BANK, CARD, etc)</param>
        /// <returns>The status of the payment</returns>
        public String checkPaymentStatus(String orderId, String provider, String providerType)
        {
            PaymentService service = Initialize();
            String status = service.checkPaymentStatus(orderId, provider, providerType);
            return status;
        }

        /// <summary>
        /// Send order information to Paylive and receive a token identifying the order
        /// </summary>
        /// <param name="orderId">Merchant's order id. Unique code identifying the order on merchant's system</param>
        /// <param name="subTotal">Total cost of items</param>
        /// <param name="deliveryCost">Cost of delivery</param>
        /// <param name="taxAmount">Amount going to tax</param>
        /// <param name="total">Total amount payable by customer</param>
        /// <param name="comment1">A short description of order</param>
        /// <param name="comment2">Any extra information about order</param>
        /// <param name="items">List of items being purchased</param>
        /// <returns>A token identifying the order on Paylive or error message in case of failure</returns>
        public String getPaymentToken(string orderId, decimal subTotal, decimal deliveryCost, decimal taxAmount, decimal total, string comment1, string comment2, OrderItem[] items)
        {
            PaymentService service = Initialize();
            string token = service.ProcessPaymentOrder(orderId, subTotal, true, deliveryCost, true, taxAmount, true, total, true, comment1, comment2, items);
            return token;
        }       

        /// <summary>
        /// Confirm receipt of transaction id from Paylive.
        /// </summary>
        /// <param name="token">Payment token returned on initial request</param>
        /// <param name="transactionId">The id of the transaction as returned via callback url</param>
        /// <returns>status indicating success or failure</returns>
        public int confirmTransaction(string token, string transactionId)
        {
            PaymentService service = Initialize();
            int result = service.ConfirmTransaction(token, transactionId);
            return result;
        }

        /// <summary>
        /// Cancel a transaction that has been processed on Paylive. Currently not implemented
        /// </summary>
        /// <param name="token">Payment token returned on initial request</param>
        /// <param name="transactionId">The id of the transaction as returned via callback url</param>
        /// <returns>status indicating success or failure</returns>
        public int cancelTransaction(string token, string transactionId)
        {
            PaymentService service = Initialize();
            int result = service.CancelTransaction(token, transactionId);
            return result;
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Instantiates the payment service and injects a PaymentHeader
        /// </summary>
        /// <returns>PaymentService</returns>
        protected PaymentService Initialize()
        {
            PaymentHeader header = new PaymentHeader();
            header.APIVersion = _apiVersion;
            header.MerchantEmail = _merchantEmail;
            header.MerchantKey = _merchantKey;
            header.SvcType = _serviceType;
            header.UseIntMode = _integrationMode;

            PaymentService service = new PaymentService();
            service.PaymentHeaderValue = header;
            return service;
        }
        #endregion
    }
}
