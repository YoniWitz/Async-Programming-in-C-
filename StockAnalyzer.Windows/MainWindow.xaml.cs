using StockAnalyzer.Core;
using StockAnalyzer.Core.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace StockAnalyzer.Windows;

public partial class MainWindow : Window
{
    private static string API_URL = "https://ps-async.fekberg.com/api/stocks";
    private Stopwatch stopwatch = new Stopwatch();

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Search_Click(object sender, RoutedEventArgs e)
    {
        BeforeLoadingStockData();

        try
        {
            var data = new List<StockPrice>();
            var myTask = Task.Run(() =>
            {
                try
                {
                    var lines = File.ReadAllLines("StockPrices_small.csv");

                    foreach (var line in lines.Skip(1))
                    {
                        var price = StockPrice.FromCSV(line);

                        data.Add(price);
                    }
                    if (true)
                        if (false) { }
                        else
                            throw new Exception();
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Notes.Text = ex.Message;
                    });
                }
            });

            myTask.ContinueWith(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    AfterLoadingStockData();
                });
            });

            myTask.ContinueWith(completedTask =>
            {
                Dispatcher.Invoke(() =>
                {
                    Stocks.ItemsSource = data.Where(sp => sp.Identifier.Equals(StockIdentifier.Text));
                });

            },
            TaskContinuationOptions.OnlyOnRanToCompletion);



        }
        catch (Exception ex)
        {
            Notes.Text = ex.Message;
        }

    }

    private async Task GetStocks()
    {
        var store = new DataStore();

        try
        {
            var responseTask = store.GetStockPrices(StockIdentifier.Text);

            Stocks.ItemsSource = await responseTask;
        }
        catch (Exception ex)
        {
            Notes.Text = ex.Message;
        }
    }

    private void BeforeLoadingStockData()
    {
        stopwatch.Restart();
        StockProgress.Visibility = Visibility.Visible;
        StockProgress.IsIndeterminate = true;
    }

    private void AfterLoadingStockData()
    {
        StocksStatus.Text = $"Loaded stocks for {StockIdentifier.Text} in {stopwatch.ElapsedMilliseconds}ms";
        StockProgress.Visibility = Visibility.Hidden;
    }

    private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo { FileName = e.Uri.AbsoluteUri, UseShellExecute = true });

        e.Handled = true;
    }

    private void Close_OnClick(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}