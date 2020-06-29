using System;
using System.Collections.Generic;
using System.Linq;
using Com.Sunmi.Pay.Hardware.Aidl;
using Com.Sunmi.Pay.Hardware.Aidlv2.Bean;
using Com.Sunmi.Pay.Hardware.Aidlv2.Emv;
using Java.Util;
using Newtonsoft.Json;
using Sunmi.Paylib;
using Xamarin.Forms;

namespace XamarinSunmiPayLibSample.Droid.PayImplementations
{
    public class EmvListener : EMVListenerV2Stub, IEMVListenerV2
    {
        private CreditCard CreditCard;
        private readonly ILogger logger;
        private enum EmvStatus
        {
            Success = 0,
            Fail = 1
        }
        private readonly Dictionary<string, List<string>> CardTypes = new Dictionary<string, List<string>>
        {
            ["UnionPay"] = new List<string> { "A000000333" },
            ["Visa"] = new List<string> { "A000000333" },
            ["Master"] = new List<string> { "A000000004", "A000000005" },
            ["AmericanExpress"] = new List<string> { "A000000025" },
            ["JCB"] = new List<string> { "A000000065" },
            ["RuPay"] = new List<string> { "A000000524" },
            ["Pure"] = new List<string> { "D999999999", "D888888888", "D777777777", "D666666666", "A000000615" }
        };
        public EmvListener()
        {
            CreditCard = new CreditCard();
            logger = DependencyService.Get<ILogger>();
        }
        public override void OnAppFinalSelect(string tag9F06value)
        {
            if (!string.IsNullOrEmpty(tag9F06value))
            {
                var paymentType = CardTypes.Where(x => x.Value.Any(y => tag9F06value.StartsWith(y))).FirstOrDefault().Key;
                logger.Log("Card Type " + paymentType == null ? "Unknown" : paymentType + " Card Detected");
            }
            else
            {
                logger.Log("OnAppFinalSelect Tag9F06value:" + tag9F06value);
                ReadAgain();
            }
            InitEmvTlvData();
            SunmiPayKernel.Instance.MEMVOptV2.ImportAppFinalSelectStatus((int)EmvStatus.Success);
        }
        private void InitEmvTlvData()
        {
            #region Normal TLV Data
            var tags = new string[2] { "5F2A", "5F36" };
            var values = new string[2] { "0356", "02" };
            SunmiPayKernel.Instance.MEMVOptV2.SetTlvList(AidlConstants.EMV.TLVOpCode.OpNormal, tags, values);
            #endregion

            #region PayPass(MasterCard) TLV Data
            var tagsPayPass = new string[15] {"DF8117", "DF8118", "DF8119", "DF811F", "DF811E", "DF812C",
                    "DF8123", "DF8124", "DF8125", "DF8126",
                    "DF811B", "DF811D", "DF8122", "DF8120", "DF8121"};
            var valuesPayPass = new string[15]{"E0", "F8", "F8", "E8", "00", "00",
                    "000000000000", "000000100000", "999999999999", "000000100000",
                    "30", "02", "0000000000", "000000000000", "000000000000"};
            SunmiPayKernel.Instance.MEMVOptV2.SetTlvList(AidlConstants.EMV.TLVOpCode.OpPaypass, tagsPayPass, valuesPayPass);
            #endregion

            #region AMEX(AmericanExpress) TLV Data
            var tagsAE = new string[8] { "9F6D", "9F6E", "9F33", "9F35", "DF8168", "DF8167", "DF8169", "DF8170" };
            var valuesAE = new string[8] { "C0", "D8E00000", "E0E888", "22", "00", "00", "00", "60" };
            SunmiPayKernel.Instance.MEMVOptV2.SetTlvList(AidlConstants.EMV.TLVOpCode.OpAe, tagsAE, valuesAE);
            #endregion
        }
        public override void OnTransResult(int code, string desc)
        {
            logger.Log(string.Format("OnTransResult Code:{0}, Descr{1} \n *******End Process*******", code, desc));
            GetExpireDateAndCardholderName();
        }
        private void GetExpireDateAndCardholderName()
        {
            var outData = new byte[64];
            var tags = new string[2] { "5F24", "5F20" };
            var len = SunmiPayKernel.Instance.MEMVOptV2.GetTlvList(AidlConstants.EMV.TLVOpCode.OpNormal, tags, outData);
            if (len > 0)
            {
                var bytesOut = Arrays.CopyOf(outData, len);
                var hexStr = Helpers.Bytes2HexStr(bytesOut);
                var map = Helpers.BuildTLVMap(hexStr);
                map.TryGetValue("5F24", out var tlv5F24); //expire date
                map.TryGetValue("5F20", out var tlv5F20); //cardholder
                var value = tlv5F20.value;
                var bytes = Helpers.GetBytesFromHexString(value);
                var cardholder = string.Empty;
                if (bytes != null)
                    cardholder = System.Text.Encoding.UTF8.GetString(bytes);
                var expireDate = tlv5F24.value;
                logger.Log("Expire Date " + expireDate);

                if (CreditCard != null && !string.IsNullOrEmpty(CreditCard.CardNumber) && !string.IsNullOrEmpty(expireDate))
                {
                    var month = expireDate.Substring(2, 2);
                    var year = expireDate.Substring(0, 2);
                    CreditCard.ExpireDate = month + " / " + year;
                    if (!string.IsNullOrEmpty(cardholder))
                    {
                        CreditCard.CardHolderName = cardholder.Replace('/', ' ');
                        logger.Log("Cardholder: " + CreditCard.CardHolderName);
                    }
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
        }
        public override void OnConfirmCardNo(string cardNo)
        {
            CreditCard = new CreditCard { CardNumber = cardNo };
            logger.Log("Card No: " + cardNo);
            SunmiPayKernel.Instance.MEMVOptV2.ImportCardNoStatus((int)EmvStatus.Success);
        }
        public override void OnRequestDataExchange(string cardNo)
        {
            logger.Log("OnRequestDataExchange Card No: " + cardNo);
            SunmiPayKernel.Instance.MEMVOptV2.ImportDataExchangeStatus((int)EmvStatus.Success);
        }
        public override void OnWaitAppSelect(IList<EMVCandidateV2> appNameList, bool isFirstSelect)
        {
            logger.Log("OnWaitAppSelect IsFirstSelect:" + isFirstSelect);
            SunmiPayKernel.Instance.MEMVOptV2.ImportAppSelect((int)EmvStatus.Success);
        }
        public override void OnCardDataExchangeComplete()
        {
            logger.Log("OnCardDataExchangeComplete");
        }
        public override void OnCertVerify(int certType, string certInfo)
        {
            logger.Log(string.Format("OnCertVerify CertType: {0}, CertInfo: {1}", certType, certInfo));
        }
        public override void OnConfirmationCodeVerified()
        {
            logger.Log("OnConfirmationCodeVerified");
        }
        public override void OnOnlineProc()
        {
            logger.Log("OnOnlineProc");
        }
        public override void OnRequestShowPinPad(int pinType, int remainTime)
        {
            logger.Log(string.Format("OnRequestShowPinPad Pin Type:{0}, Remain Time:{1}", pinType, remainTime));
        }
        public override void OnRequestSignature()
        {
            logger.Log("OnRequestSignature");
        }
        private void ReadAgain()
        {
            logger.Log("ReadAgain");
        }
    }
}
