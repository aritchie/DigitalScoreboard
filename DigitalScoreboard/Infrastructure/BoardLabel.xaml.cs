namespace DigitalScoreboard;


public partial class BoardLabel : ContentView
{
	public BoardLabel()
	{
		this.InitializeComponent();
	}


    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title),
        typeof(string),
        typeof(BoardLabel),
        null
    );
    public string Title
    {
        get => (string)this.GetValue(TitleProperty);
        set => this.SetValue(TitleProperty, value);
    }


    public static readonly BindableProperty InfoProperty = BindableProperty.Create(
        nameof(Info),
        typeof(string),
        typeof(BoardLabel),
        null
    );
    public string Info
    {
        get => (string)this.GetValue(InfoProperty);
        set => this.SetValue(InfoProperty, value);
    }


    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(BoardLabel),
        null
    );
    public ICommand Command
    {
        get => (ICommand)this.GetValue(CommandProperty);
        set => this.SetValue(CommandProperty, value);
    }


    protected override void OnPropertyChanged(string? propertyName = null)
    {
        if (propertyName == TitleProperty.PropertyName)
        {
            this.lblTitle.Text = this.Title;
            this.lblTitle.IsVisible = !String.IsNullOrWhiteSpace(this.Title);
        }
        else if (propertyName == InfoProperty.PropertyName)
        {
            this.lblInfo.Text = this.Info;
        }
        else if (propertyName == CommandProperty.PropertyName)
        {
            grCommand.Command = this.Command;
        }
    }
}
