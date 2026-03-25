using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OnlineLearningPlatformAss2.Service.DTOs.VnPay;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;

namespace OnlineLearningPlatformAss2.Service.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<VnPayService> _logger;

        public VnPayService(IConfiguration configuration, ILogger<VnPayService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string CreatePaymentUrl(HttpContext context, VnPayRequestModel model)
        {
            _logger.LogInformation("--- VnPayService.CreatePaymentUrl ---");
            _logger.LogInformation("OrderId: {OrderId}, Amount: {Amount}", model.OrderId, model.Amount);
            _logger.LogInformation("VnPay:TmnCode: {TmnCode}", _configuration["VnPay:TmnCode"]);
            _logger.LogInformation("VnPay:HashSecret: {HashSecret}", _configuration["VnPay:HashSecret"] != null ? "***" : "null"); // Hide secret in logs
            _logger.LogInformation("VnPay:BaseUrl: {BaseUrl}", _configuration["VnPay:BaseUrl"]);
            _logger.LogInformation("VnPay:CallbackUrl: {CallbackUrl}", _configuration["VnPay:CallbackUrl"]);

            Console.WriteLine("aaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            var vnpayData = new SortedDictionary<string, string>();

            vnpayData.Add("vnp_Version", _configuration["VnPay:Version"]);
            vnpayData.Add("vnp_Command", "pay");
            vnpayData.Add("vnp_TmnCode", _configuration["VnPay:TmnCode"]);
            vnpayData.Add("vnp_Amount", (model.Amount * 100).ToString());
            vnpayData.Add("vnp_CurrCode", "VND");
            vnpayData.Add("vnp_TxnRef", model.OrderId.ToString());
            vnpayData.Add("vnp_OrderInfo", model.OrderDescription);
            vnpayData.Add("vnp_OrderType", _configuration["VnPay:OrderType"]);
            vnpayData.Add("vnp_Locale", string.IsNullOrEmpty(model.Locale) ? "vn" : model.Locale);
            var callbackUrl = _configuration["VnPay:CallbackUrl"];
            if (callbackUrl != null && callbackUrl.StartsWith("/"))
            {
                // Dynamic callback based on current request
                callbackUrl = $"{context.Request.Scheme}://{context.Request.Host}{callbackUrl}";
            }
            vnpayData.Add("vnp_ReturnUrl", callbackUrl);
            vnpayData.Add("vnp_IpAddr", GetIpAddress(context));
            vnpayData.Add("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));

            if (!string.IsNullOrEmpty(model.BankCode))
            {
                vnpayData.Add("vnp_BankCode", model.BankCode);
            }

            if (model.ExpireDate.HasValue)
            {
                vnpayData.Add(
                    "vnp_ExpireDate",
                    model.ExpireDate.Value.ToString("yyyyMMddHHmmss")
                );
            }

            string queryString = BuildQueryString(vnpayData);
            string secureHash = HmacSHA512(
                _configuration["VnPay:HashSecret"],
                queryString
            );
            
            _logger.LogInformation("QueryString: {QueryString}", queryString);
            _logger.LogInformation("SecureHash: {SecureHash}", secureHash);

            var paymentUrl = $"{_configuration["VnPay:BaseUrl"]}?{queryString}&vnp_SecureHash={secureHash}";
            _logger.LogInformation("Generated Payment URL: {PaymentUrl}", paymentUrl);
            _logger.LogInformation("-------------------------------------");

            return paymentUrl;
        }

        public VnPayResponseModel PaymentExecute(IQueryCollection collections)
        {
            _logger.LogInformation("--- VnPayService.PaymentExecute ---");
            var responseData = new SortedDictionary<string, string>();

            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    responseData.Add(key, value);
                    _logger.LogInformation("Received {Key}: {Value}", key, value);
                }
            }

            string receivedHash = responseData["vnp_SecureHash"];
            responseData.Remove("vnp_SecureHash");
            responseData.Remove("vnp_SecureHashType");

            string rawData = BuildQueryString(responseData);
            string calculatedHash = HmacSHA512(
                _configuration["VnPay:HashSecret"],
                rawData
            );
            
            _logger.LogInformation("RawData for signature: {RawData}", rawData);
            _logger.LogInformation("Received Hash: {ReceivedHash}", receivedHash);
            _logger.LogInformation("Calculated Hash: {CalculatedHash}", calculatedHash);

            bool isValidSignature = receivedHash.Equals(
                calculatedHash,
                StringComparison.InvariantCultureIgnoreCase
            );
            
            _logger.LogInformation("Signature Valid: {IsValidSignature}", isValidSignature);
            
            var responseCode = responseData.ContainsKey("vnp_ResponseCode") ? responseData["vnp_ResponseCode"] : "null";
            _logger.LogInformation("VNPay Response Code: {ResponseCode}", responseCode);
            _logger.LogInformation("-----------------------------------");

            return new VnPayResponseModel
            {
                Success = isValidSignature && responseData["vnp_ResponseCode"] == "00",
                PaymentMethod = "VNPAY",
                OrderId = responseData["vnp_TxnRef"],
                TransactionId = responseData["vnp_TransactionNo"],
                VnPayResponseCode = responseData["vnp_ResponseCode"],
                Message = responseData.ContainsKey("vnp_Message") ? responseData["vnp_Message"] : null
            };
        }

        private static string BuildQueryString(
            SortedDictionary<string, string> data
        )
        {
            var builder = new StringBuilder();
            foreach (var item in data)
            {
                if (!string.IsNullOrEmpty(item.Value))
                {
                    builder.Append(WebUtility.UrlEncode(item.Key));
                    builder.Append('=');
                    builder.Append(WebUtility.UrlEncode(item.Value));
                    builder.Append('&');
                }
            }
            return builder.ToString().TrimEnd('&');
        }

        private static string HmacSHA512(string key, string input)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            byte[] hashValue = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
        }

        private static string GetIpAddress(HttpContext context)
        {
            return context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        }
    }
}
