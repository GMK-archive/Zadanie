using System.Data;
using System.Net.Http.Json;
using System.Web;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        DateTime lastChatRefresh;
        IDispatcherTimer timer;
        public MainPage()
        {
            InitializeComponent();
            lastChatRefresh = DateTime.UnixEpoch;

            timer = Application.Current.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                GetHistory();
            };
            timer.Start();
        }

        private void Send(object sender, EventArgs e)
        {
            string username = UsernameEntry.Text;
            if (string.IsNullOrWhiteSpace(username))
            {
                DisplayAlert("Error", "Nie można wysać wiadomości", "OK");
                return;
            }
            string message = ChatEntry.Text;
            if (string.IsNullOrWhiteSpace(message))
            {
                DisplayAlert("Error", "Nie można wysac pustej wiadomości", "OK");
                return;
            }

            ChatEntry.Text = string.Empty;
            //Label NewMessageLabel = new Label();
            //NewMessageLabel.Text = username + ": " + message;
            //ChatHistory.Children.Add(NewMessageLabel);
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:5038");
                client.DefaultRequestHeaders.Add("Authorization", "Bearer superTajnyToken");
                ChatMessage chatMessage = new ChatMessage
                {
                    Autor = username,
                    Content = message,
                    Timestamp = DateTime.Now
                };
                HttpResponseMessage response = client.PostAsJsonAsync("/chat/messages", chatMessage).Result;
                if (response.IsSuccessStatusCode)
                {
                    //Message sent successfully
                }
                else
                {
                    DisplayAlert("Error", "Failed to send message", "OK");
                }
            }
        }
        private void GetHistory()
        {
            string timestamp = lastChatRefresh.ToString("o");
            timestamp = HttpUtility.UrlEncode(timestamp);
            //blok using żeby automatycznie zamknąć klienta po zakończeniu
            using (HttpClient client = new HttpClient())
            {
                //ustawienie adresu bazowego
                client.BaseAddress = new Uri("http://localhost:5038/");
                client.DefaultRequestHeaders.Add("Authorization", "Bearer superTajnyToken");
                //wykonanie żądania GET do endpointu /chat
                HttpResponseMessage response = client.GetAsync("/chat/messages?minimalDate=" + timestamp).Result;
                //parsuj odpowiedź jako listę obiektów typu ChatMessage
                List<ChatMessage> messages = response.Content.ReadFromJsonAsync<List<ChatMessage>>().Result
                                                                ?? new List<ChatMessage>();
                //dopisujemy je do historii czatu
                foreach (ChatMessage message in messages)
                {
                    Label messageLabel = new Label();
                    messageLabel.Text = message.Timestamp + " " + message.Autor + ": " + message.Content;
                    ChatHistory.Children.Add(messageLabel);
                }
            }
            ChatScrollView.ScrollToAsync(ChatHistory, ScrollToPosition.End, true);
            lastChatRefresh = DateTime.Now;
        }
    }
}
