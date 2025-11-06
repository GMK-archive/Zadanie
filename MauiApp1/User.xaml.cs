namespace MauiApp1;

public partial class User : ContentPage
{
	public User()
	{
		InitializeComponent();
	}

    private void UserEntryButton_Clicked(object sender, EventArgs e)
    {
		DisplayAlert("debug test", LoginEntry.Text + " " + PasswordEntry.Text, "Test");
    }
}