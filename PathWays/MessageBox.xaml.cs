using CommunityToolkit.Maui.Views;

namespace PathWays;

public partial class MessageBox : Popup
{

	public MessageBox(string message )
	{
        InitializeComponent();
        label.Text = message;
    }
}