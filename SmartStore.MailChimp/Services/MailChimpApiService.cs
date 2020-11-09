using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using MailChimp;
using MailChimp.Helper;
using MailChimp.Lists;
using SmartStore.Core.Logging;
using SmartStore.MailChimp.Data;

namespace SmartStore.MailChimp.Services
{
    public class MailChimpApiService : IMailChimpApiService
    {
        private readonly MailChimpSettings _mailChimpSettings;
        private readonly ISubscriptionEventQueueingService _subscriptionEventQueueingService;
        private readonly ILogger _log;

        public MailChimpApiService(MailChimpSettings mailChimpSettings, ISubscriptionEventQueueingService subscriptionEventQueueingService, ILogger log)
        {
            _mailChimpSettings = mailChimpSettings;
            _subscriptionEventQueueingService = subscriptionEventQueueingService;
            _log = log;
        }

        /// <summary>
        /// Retrieves the lists.
        /// </summary>
        /// <returns></returns>
        public virtual NameValueCollection RetrieveLists()
        {
            var output = new NameValueCollection();
            try
            {
                // Pass the API key on the constructor:
                MailChimpManager mc = new MailChimpManager(_mailChimpSettings.ApiKey);

                // Next, make any API call you'd like:
                ListResult lists = mc.GetLists();

                if (lists != null && lists.Data.Count > 0)
                {
                    foreach (var item in lists.Data)
                    {
                        output.Add(item.Name, item.Id);
                    }
                }
            }
            catch (Exception e)
            {
                _log.Debug(e, e.Message);
            }
            return output;
        }

        /// <summary>
        /// Batches the unsubscribe.
        /// </summary>
        /// <param name="recordList">The records</param>
        public virtual BatchUnsubscribeResult BatchUnsubscribe(IEnumerable<MailChimpEventQueueRecord> recordList)
        {
            if (String.IsNullOrEmpty(_mailChimpSettings.DefaultListId))
                throw new ArgumentException("MailChimp list is not specified");

            MailChimpManager mc = new MailChimpManager(_mailChimpSettings.ApiKey);

            //  Create the email parameter
            List<EmailParameter> emailList = new List<EmailParameter>();
            var batch = recordList.Select(sub => sub.Email).ToList();

            foreach (var email in batch)
            {
                emailList.Add(new EmailParameter() { Email = email });
            }

            BatchUnsubscribeResult listSubscribeOutput = mc.BatchUnsubscribe(_mailChimpSettings.DefaultListId, emailList);
            return listSubscribeOutput;
        }

        /// <summary>
        /// Batches the subscribe.
        /// </summary>
        /// <param name="recordList">The records</param>
        public virtual BatchSubscribeResult BatchSubscribe(IEnumerable<MailChimpEventQueueRecord> recordList)
        {
            if (String.IsNullOrEmpty(_mailChimpSettings.DefaultListId))
                throw new ArgumentException("MailChimp list is not specified");

            MailChimpManager mc = new MailChimpManager(_mailChimpSettings.ApiKey);

            List<BatchEmailParameter> emailList = new List<BatchEmailParameter>();

            foreach (var sub in recordList)
            {
                emailList.Add(new BatchEmailParameter()
                {
                    Email = new EmailParameter()
                    {
                        Email = sub.Email
                    }
                });
            }

            BatchSubscribeResult listSubscribeOutput = mc.BatchSubscribe(_mailChimpSettings.DefaultListId, emailList, false, true);
            return listSubscribeOutput;
        }

        public virtual SyncResult Synchronize()
        {
            var result = new SyncResult();

            // Get all the queued records for subscription/unsubscription
            var allRecords = _subscriptionEventQueueingService.GetAll();
            //get unique and latest records
            var allRecordsUnique = new List<MailChimpEventQueueRecord>();
            foreach (var item in allRecords
                .OrderByDescending(x => x.CreatedOnUtc))
            {
                var exists = allRecordsUnique
                    .Where(x => x.Email.Equals(item.Email, StringComparison.InvariantCultureIgnoreCase))
                    .FirstOrDefault() != null;
                if (!exists)
                    allRecordsUnique.Add(item);
            }
            var subscribeRecords = allRecordsUnique.Where(x => x.IsSubscribe).ToList();
            var unsubscribeRecords = allRecordsUnique.Where(x => !x.IsSubscribe).ToList();

            //subscribe
            if (subscribeRecords.Count > 0)
            {
                var subscribeResult = BatchSubscribe(subscribeRecords);
                //result
                if (subscribeResult.ErrorCount > 0)
                {
                    foreach (var error in subscribeResult.Errors)
                        result.SubscribeErrors.Add(error.ErrorMessage);
                }
                else
                {
                    result.SubscribeResult = subscribeResult.UpdateCount.ToString();
                }
            }
            else
            {
                result.SubscribeResult = "No records to add";
            }
            //unsubscribe
            if (unsubscribeRecords.Count > 0)
            {
                var unsubscribeResult = BatchUnsubscribe(unsubscribeRecords);
                //result
                if (unsubscribeResult.ErrorCount > 0)
                {
                    foreach (var error in unsubscribeResult.Errors)
                        result.UnsubscribeErrors.Add(error.ErrorMessage);
                }
                else
                {
                    result.UnsubscribeResult = unsubscribeResult.SuccessCount.ToString();
                }
            }
            else
            {
                result.UnsubscribeResult = "No records to unsubscribe";
            }

            //delete the queued records
            foreach (var sub in allRecords)
                _subscriptionEventQueueingService.Delete(sub);

            //other useful properties of listBatchSubscribeOutput and listBatchUnsubscribeOutput
            //output.result.add_count
            //output.result.error_count
            //output.result.update_count
            //output.result.errors
            //output.api_Request, output.api_Response, // raw data
            //output.api_ErrorMessages, output.api_ValidatorMessages); // & errors
            return result;
        }
    }
}