using System.Windows.Input;
using Xamarin.Forms;
using XamarinSunmiPayLibSample.Helpers;

namespace XamarinSunmiPayLibSample
{
    public class MainPageViewModel : BindableBase
    {
        public ICommand ReadCommand { get; set; }
        public ICommand PrintCommand { get; set; }
        private CreditCard _creditCard;
        public CreditCard CreditCard
        {
            get { return _creditCard; }
            set { _creditCard = value; OnPropertyChanged(); }
        }
        private bool _isRunning;
        public bool IsRunning
        {
            get { return _isRunning; }
            set { _isRunning = value; OnPropertyChanged(); }
        }
        private readonly IPayService payService;
        public MainPageViewModel()
        {
            payService = DependencyService.Get<IPayService>();
            ReadCommand = new Command((e) =>
            {
                IsRunning = true;
                CreditCard = new CreditCard();
                switch ((string)e)
                {
                    case "Nfc":
                        payService.Read(ReadType.Nfc);
                        break;
                    case "Ic":
                        payService.Read(ReadType.Ic);
                        break;
                    case "Magnetic":
                        payService.Read(ReadType.Magnetic);
                        break;
                    case "All":
                        payService.Read(ReadType.Nfc | ReadType.Ic | ReadType.Magnetic);
                        break;
                }
            });
            PrintCommand = new Command(() =>
            {
                if (CreditCard == null)
                    return;
                var entry = new PrintableDocument();
                var print = entry.GenerateDocument(CreditCard);
                DependencyService.Get<IPrinterService>().Print(print);
            });
            MessagingCenter.Instance.Unsubscribe<CreditCard>("PaymentCallback", "CreditCardReaded");
            MessagingCenter.Instance.Subscribe<CreditCard>("PaymentCallback", "CreditCardReaded", (card) =>
            {
                if (string.IsNullOrWhiteSpace(card.CardHolderName))
                    card.CardHolderName = "Unreaded";
                CreditCard = card;
                IsRunning = false;
            });
        }
    }
}
