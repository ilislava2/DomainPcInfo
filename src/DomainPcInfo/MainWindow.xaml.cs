using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DomainPcInfo;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _refreshTimer;
    private bool _refreshInProgress;

    public MainWindow()
    {
        InitializeComponent();

        Loaded += MainWindow_Loaded;
        SizeChanged += MainWindow_SizeChanged;

        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(5)
        };

        _refreshTimer.Tick += RefreshTimer_Tick;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Сначала получаем все данные.
        // Это важно при SizeToContent="WidthAndHeight":
        // до заполнения текста реальная ширина окна еще неизвестна.
        await RefreshAsync();

        // Заставляем WPF пересчитать фактический размер окна.
        UpdateLayout();

        // После этого ставим окно в верхний правый угол.
        PositionAtTopRight();

        _refreshTimer.Start();
    }

    private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Если длина текста изменилась, появился второй сетевой
        // интерфейс и т.п. — окно останется прижатым вправо.
        if (IsLoaded)
        {
            PositionAtTopRight();
        }
    }

    private async void RefreshTimer_Tick(object? sender, EventArgs e)
    {
        await RefreshAsync();
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        if (_refreshInProgress)
            return;

        _refreshInProgress = true;
        RefreshButton.IsEnabled = false;

        try
        {
            // -------------------------------------------------
            // Имя компьютера
            // -------------------------------------------------

            ComputerNameText.Text =
                $"Имя ПК: {SystemInfo.ComputerName}";

            // -------------------------------------------------
            // Домен / рабочая группа
            // -------------------------------------------------

            ComputerJoinInfo join = SystemInfo.GetJoinInfo();

            JoinText.Text = join.JoinType switch
            {
                ComputerJoinType.Domain =>
                    $"Домен: {join.Name}",

                ComputerJoinType.Workgroup =>
                    $"Рабочая группа: {join.Name}",

                _ =>
                    $"Группа/домен: {join.Name}"
            };

            // -------------------------------------------------
            // Контроллер домена
            // -------------------------------------------------

            if (join.JoinType == ComputerJoinType.Domain)
            {
                DcAvailabilityText.Text =
                    "Доступ DC: Проверка...";

                DomainControllerStatus dc =
                    await DomainInfo.GetStatusAsync();

                DcText.Text =
                    $"DC: {dc.ControllerName}";

                DcAvailabilityText.Text =
                    $"Доступ DC: {(dc.IsAvailable ? "Доступно" : "Недоступно")}";
            }
            else
            {
                DcText.Text = "DC: —";
                DcAvailabilityText.Text =
                    "Доступ DC: Недоступно";
            }

            // -------------------------------------------------
            // Физические сетевые интерфейсы
            // -------------------------------------------------

            IReadOnlyList<PhysicalNetworkAdapterInfo> adapters =
                NetworkInfo.GetActivePhysicalIPv4Adapters();

            DrawNetworkAdapters(adapters);

            // Пересчитываем размер после изменения текста.
            UpdateLayout();

            // И снова гарантированно прижимаем к углу.
            PositionAtTopRight();
        }
        catch (Exception ex)
        {
            // Пока логирование является заглушкой.
            AppLog.Write(ex.ToString());

            DcAvailabilityText.Text =
                "Доступ DC: Недоступно";
        }
        finally
        {
            RefreshButton.IsEnabled = true;
            _refreshInProgress = false;
        }
    }

    private void DrawNetworkAdapters(
        IReadOnlyList<PhysicalNetworkAdapterInfo> adapters)
    {
        NetworkPanel.Children.Clear();

        if (adapters.Count == 0)
        {
            NetworkPanel.Children.Add(
                CreateInfoText(
                    "Сетевые интерфейсы: не найдены"));

            return;
        }

        foreach (PhysicalNetworkAdapterInfo adapter in adapters)
        {
            string addresses =
                string.Join(", ", adapter.IPv4Addresses);

            NetworkPanel.Children.Add(
                CreateInfoText(
                    $"{adapter.Name}: {addresses}"));
        }
    }

    private static TextBlock CreateInfoText(string text)
    {
        return new TextBlock
        {
            Text = text,

            // FontFamily, FontSize, FontWeight и Foreground
            // специально здесь не задаём.
            //
            // Они наследуются от MainWindow.xaml,
            // поэтому строки сетевых интерфейсов будут
            // выглядеть точно так же, как остальные строки.

            Margin = new Thickness(
                0,
                1,
                0,
                1)
        };
    }

    private void PositionAtTopRight()
    {
        Rect workArea = SystemParameters.WorkArea;

        // Почти вплотную к правому верхнему углу.
        const double rightMargin = 3;
        const double topMargin = 1;

        Left =
            workArea.Right -
            ActualWidth -
            rightMargin;

        Top =
            workArea.Top +
            topMargin;
    }
}