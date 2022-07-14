namespace DigitalScoreboard;


public partial class App : Application
{
	public App()
	{
		this.InitializeComponent();

		FillLabel.Wireup();
	}
}
