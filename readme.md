# Digital Scoreboard

A football scoreboard that can be controlled through Bluetooth.  Great for little league games.

## Features
* Referee "client" screen that allows you to control the scoreboard remotely
* Customized Team Names
* Customized Game & Play Clock Timers
* Full Screen Clock for Half Time or Play Clock
* Screen Properties

    * Current quarter/period
    * Current Down
    * Play Clock
    * Game Clock

## The "Stack"
* .NET MAUI
* Prism
* ReactiveUI
* Shiny (BluetoothLE & BluetoothLE Hosting)
* Shiny.Framework (brings together Shiny, Prism, & ReactiveUI in a tidy little bundle)

## Scoreboard
This screen is meant for tablets in landscape view.  It sets up a bluetooth LE host using Shiny.BluetoothLE.Hosting that can allow connections to control the scoreboard clocks and scores

You can also control most settings, pause/resume of clocks, etc by tapping on the screen.  This is a good option if you don't have a tablet and a phone to remote control with or if bluetooth distance is a problem.

<img src="scoreboard.png" />

## Referee

Uses Shiny.BluetoothLE to connect to the scoreboard host.

<img src="referee.png" />

## Settings

The settings screen allows you to set all of the scoreboard constraints and others things like team names

<img src="settings.png" />

## TODO

* Timeouts Remaining on Scoreboard
* Ball Possession on Scoreboard
* Online Error Log - AppCenter, Firebase, Sentry, etc
* Yards-to-go for 1st down
* Timeouts Remaining
* Remember Game per session (start new or resume)
* Better scoreboard design


<ContentPage.Resources>
        <ResourceDictionary>
            <Style TargetType="VerticalStackLayout">
                <Setter Property="VerticalOptions" Value="FillAndExpand" />
                <Setter Property="HorizontalOptions" Value="FillAndExpand" />
            </Style>

            <Style x:Key="Main" TargetType="Border">
                <Setter Property="Stroke" Value="White" />
                <Setter Property="StrokeThickness" Value="10" />
            </Style>
            <!--<Style TargetType="Frame">
                <Setter Property="BackgroundColor" Value="Black" />
                <Setter Property="BorderColor" Value="White" />
                <Setter Property="CornerRadius" Value="10" />
            </Style>-->
            <Style x:Key="Title" TargetType="Label">
                <Setter Property="FontSize" Value="70" />
                <Setter Property="TextColor" Value="White" />
                <Setter Property="FontFamily" Value="{Binding Font}" />
                <Setter Property="HorizontalTextAlignment" Value="Center" />
            </Style>

            <Style x:Key="Info" TargetType="Label">
                <Setter Property="FontSize" Value="88" />
                <Setter Property="TextColor" Value="Yellow" />
                <Setter Property="FontFamily" Value="{Binding Font}" />
                <Setter Property="HorizontalTextAlignment" Value="Center" />
            </Style>
        </ResourceDictionary>
    </ContentPage.Resources>

    <ContentPage.Content>
        <Border Margin="0" Style="{StaticResource Main}">

            <Grid RowDefinitions="1*, 1*" ColumnDefinitions="1*, 2*, 1*">

                <VerticalStackLayout Grid.Row="0" Grid.Column="0">
                    <!--
                    <TapGestureRecognizer Command="{Binding SetHomeScore}" />
                    <Label Text="{Binding HomeTeamName}" />
                    <Label Text="{Binding HomeTeamScore}" />
                    -->
                </VerticalStackLayout>

                <VerticalStackLayout Grid.Row="0" Grid.Column="1">
                    <Border Style="{StaticResource Main}" >
                        <!--
                        <TapGestureRecognizer Command="{Binding TogglePeriodClock}" />

                        <Label Text="{Binding PeriodClock, StringFormat='{0:c}'}" />
                        -->
                    </Border>


                    <!--
                    <TapGestureRecognizer Command="{Binding IncrementPeriod}" />
                    <Label Text="{Binding Period}" />
                    -->
                </VerticalStackLayout>

                <VerticalStackLayout Grid.Row="0" Grid.Column="2">
                    <!--
                    <TapGestureRecognizer Command="{Binding SetAwayScore}" />
 
                    <Label Text="{Binding AwayTeamName}" />

                    <Label Text="{Binding AwayTeamScore}" />
                    -->
                </VerticalStackLayout>

                <VerticalStackLayout Grid.Row="1" Grid.Column="0">
                    <!--
                    <TapGestureRecognizer Command="{Binding IncrementDown}" />
                    <Label Text="{Binding Down}" />
                    -->

                </VerticalStackLayout>

                <VerticalStackLayout Grid.Row="1" Grid.Column="1">
                    <!--yards to go-->
                    <!--ball on the X-->
                </VerticalStackLayout>

                <VerticalStackLayout Grid.Row="1" Grid.Column="2">
                    <!--
                    <TapGestureRecognizer Command="{Binding TogglePlayClock}" />
                    <Label Text="{Binding PlayClock}" />
                    -->
                </VerticalStackLayout>
            </Grid>
        </Border>
    </ContentPage.Content>