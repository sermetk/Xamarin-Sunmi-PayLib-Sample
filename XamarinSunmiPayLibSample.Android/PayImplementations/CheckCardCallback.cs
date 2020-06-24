using System;
using System.Linq;
using Android.OS;
using Com.Sunmi.Pay.Hardware.Aidl;
using Com.Sunmi.Pay.Hardware.Aidlv2.Bean;
using Com.Sunmi.Pay.Hardware.Aidlv2.Readcard;
using Newtonsoft.Json;
using Sunmi.Paylib;
using Xamarin.Forms;

namespace XamarinSunmiPayLibSample.Droid.PayImplementations
{
    public class CheckCardCallback : CheckCardCallbackV2Stub, ICheckCardCallbackV2
    {
        private CreditCard CreditCard;
        private readonly ILogger logger;
        public CheckCardCallback()
        {
            logger = DependencyService.Get<ILogger>();
        }
        public override void FindMagCard(Bundle bundle)
        {
            ParseMagneticCardBundle(bundle);
        }
        private void ParseMagneticCardBundle(Bundle bundle)
        {
            var track1 = bundle.GetString("TRACK1");
            var track2 = bundle.GetString("TRACK2");
            if (!string.IsNullOrEmpty(track2))
            {
                var index = track2.IndexOf("=");
                if (index != -1)
                {
                    var cardNo = track2.JavaSubString(0, index);
                    logger.Log("Card No: " + cardNo);
                    CreditCard = new CreditCard { CardNumber = cardNo };
                }
                var expireDate = track2.Substring(index + 1, 4);
                logger.Log("Expire Date: " + expireDate);
                var month = expireDate.Substring(2, 2);
                var year = expireDate.Substring(0, 2);
                if (CreditCard != null)
                    CreditCard.ExpireDate = month + " / " + year;
            }
            if (!string.IsNullOrEmpty(track1))
            {
                var index = track1.IndexOf("^");
                var endIndex = track1.IndexOf(" ");
                if (index != -1 && endIndex != -1)
                {
                    var cardholder = track1.JavaSubString(index + 1, endIndex);
                    if (!string.IsNullOrEmpty(cardholder) && CreditCard != null)
                    {
                        CreditCard.CardHolderName = cardholder.Replace('/', ' ');
                        logger.Log("Cardholder: " + CreditCard.CardHolderName);
                    }
                }
            }
            if (CreditCard != null && !string.IsNullOrEmpty(CreditCard.CardNumber) && !string.IsNullOrEmpty(CreditCard.ExpireDate))
            {
                MessagingCenter.Instance.Send(CreditCard, "CreditCardReaded");
            }
            else
            {
                ReadAgain();
                if (CreditCard != null)
                {
                    var nullOrEmptyFields = CreditCard.GetType().GetProperties().Where(x => string.IsNullOrEmpty((string)x.GetValue(CreditCard))).Select(x => x.Name);
                    logger.Log("Unreaded field(s):" + JsonConvert.SerializeObject(nullOrEmptyFields), new MissingFieldException());
                }
            }
        }
        public override void FindICCard(string atr)
        {
            logger.Log("FindICCard Atr:" + atr);
            TransactProcess(AidlConstants.CardType.Ic.Value);
        }
        public override void FindRFCard(string uuid)
        {
            logger.Log("FindRFCard UUID:" + uuid);
            TransactProcess(AidlConstants.CardType.Nfc.Value);
        }
        public void TransactProcess(int cardType)
        {
            logger.Log("TransactProcess CardType:" + cardType);
            var emvTransData = new EMVTransDataV2 { CardType = cardType, Amount = "0", FlowType = AidlConstants.CardExistStatus.CardPresent };
            SunmiPayKernel.Instance.MEMVOptV2.TransactProcess(emvTransData, new EmvListener());
        }
        public override void OnError(int code, string message)
        {
            ReadAgain();
            logger.Log("OnError " + code + "  " + message);
        }
        public override void FindRFCardEx(Bundle info)
        {
            logger.Log("FindRFCardEx Info:" + info.BundletoString());
        }
        public override void FindICCardEx(Bundle info)
        {
            logger.Log("FindICCardEx Info:" + info.BundletoString());
        }
        public override void OnErrorEx(Bundle info)
        {
            logger.Log("OnErrorEx Info:" + info.BundletoString());
        }
        private void ReadAgain()
        {
            logger.Log("ReadAgain");
        }
    }
}
