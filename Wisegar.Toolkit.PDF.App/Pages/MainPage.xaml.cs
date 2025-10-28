using System.Collections.ObjectModel;
using System.Diagnostics;
using WG.PdfTools.Pages; // Assicurati che questo namespace sia corretto se MainPageService è presente
using System.Collections.Generic;
using Microsoft.Extensions.Logging; // Aggiunto per List<string> e Any()

namespace WG.PdfTools
{
    public partial class MainPage : ContentPage
    {
        private readonly ObservableCollection<string> items = new ObservableCollection<string>();
        private readonly MainPageService pageService;
        private readonly ILogger<MainPage> _logger;

        public MainPage(ILogger<MainPage> logger, ILogger<MainPageService> loggerMainPageService)
        {
            InitializeComponent();
            _logger = logger; 
            pageService = new MainPageService(loggerMainPageService);
            FilesToMergeListView.ItemsSource = items;

            // Iscriviti all'evento CollectionChanged di ObservableCollection
            items.CollectionChanged += Items_CollectionChanged;

            // Chiama inizialmente il metodo per impostare lo stato dei pulsanti
            UpdateButtonStates();
        }

        // Metodo per aggiornare lo stato dei pulsanti
        private void UpdateButtonStates()
        {
            bool hasItems = items.Count > 0;
            MergeBtn.IsEnabled = hasItems; // Abilita se ci sono elementi
           // CancelBtn.IsEnabled = hasItems; // Abilita se ci sono elementi
            ClearBtn.IsEnabled = hasItems; // Abilita se ci sono elementi
        }

        // Gestore eventi CollectionChanged
        private void Items_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) // 'oggetto?' per la nullità
        {
            UpdateButtonStates(); // Ogni volta che la raccolta cambia (aggiunge/rimuove), aggiorna i pulsanti
        }

        // Assicurati di annullare l'iscrizione all'evento quando la pagina viene distrutta per evitare perdite di memoria
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            items.CollectionChanged -= Items_CollectionChanged;
        }

        private async void OnMergeClicked(object sender, EventArgs e)
        {
            try { 
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            _logger.LogDebug("Percorso del desktop: {desktopPath}", desktopPath);
                if (string.IsNullOrEmpty(desktopPath))
            {
                await Shell.Current.DisplayAlert("WG PDFToolkit", "Impossibile recuperare il percorso del desktop di Windows.", "OK");
                _logger.LogError("Impossibile recuperare il percorso del desktop di Windows.");
                    return;
            }
            if (items.Count == 0)
            {
                await Shell.Current.DisplayAlert("WG PDFToolkit", "Nessun file è stato caricato.", "OK");
                _logger.LogError("Nessun file è stato caricato per la fusione.");
                    return;
            }
            var today = DateTime.Now;
            var outputFile = Path.Combine(desktopPath, $"wg-pdftoolkit-merged-file-{today.Year}{today.Month}{today.Day}{today.Hour}{today.Minute}.pdf");
            _logger.LogDebug("File di output: {outputFile}", outputFile);
                var result = pageService.MergeFiles(new List<string>(items), outputFile);

            if (result == true)
            {
                bool openFile = await Shell.Current.DisplayAlert(
                    "WG PDFToolkit",
                    $"File di output generato correttamente:\n{outputFile}\n\nVuoi aprire il file?",
                    "Apri file",
                    "OK"
                );
                items.Clear();

                if (openFile)
                {
#if WINDOWS
                    try
                    {
                        var psi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = outputFile,
                            UseShellExecute = true
                        };
                        System.Diagnostics.Process.Start(psi);
                    }
                    catch (Exception ex)
                    {
                        await Shell.Current.DisplayAlert("Errore", $"Impossibile aprire il file:\n{ex.Message}", "OK");
                        _logger.LogError("Impossibile aprire il file: {message}", ex.Message);
                    }
#endif
                    }
                }
            else
            {
                await Shell.Current.DisplayAlert("INFO WG PDFToolkit", "Impossibile elaborare i file.", "OK");
                    _logger.LogError("Impossibile elaborare i file durante la fusione.");
                }
            }
            catch(Exception ex) {
                await Shell.Current.DisplayAlert("INFO WG PDFToolkit", $"Impossibile elaborare i file.Ex. {ex.Message}", "OK");
                _logger.LogError("Impossibile elaborare i file durante la fusione. Eccezione: {message}", ex.Message);
            }
        }

        // Questo metodo verrà chiamato quando si fa clic sul pulsante "Elimina" su un elemento
        private void OnRemoveFileClicked(object sender, EventArgs e)
        {
            // Il CommandParameter del pulsante ci passa il percorso del file
            if (sender is ImageButton button && button.CommandParameter is string filePath)
            {
                // Rimuovi il file dalla raccolta
                items.Remove(filePath);
                Debug.WriteLine($"File eliminato: {filePath}");
            }
        }

        // Questo metodo verrà chiamato quando si fa clic sul pulsante "Cancella elenco"
        private async void OnClearClicked(object sender, EventArgs e)
        {
            // Chiedi all'utente se desidera davvero pulire
            bool answer = await DisplayAlert("Conferma", "Sei sicuro di voler cancellare tutti i documenti nell'elenco?", "Sì", "No");
            if (answer)
            {
                items.Clear(); // Cancella tutti gli elementi nella raccolta
                Debug.WriteLine("L'elenco dei file è stato cancellato tramite il pulsante 'Cancella elenco'.");
            }
        }

        // Questo metodo verrà chiamato quando si fa clic sul pulsante generale "Annulla"
        private async void OnCancelClicked(object sender, EventArgs e)
        {
            // Chiedi all'utente se desidera davvero annullare/cancellare
            bool answer = await DisplayAlert("Conferma", "Sei sicuro di voler annullare e cancellare l'elenco?", "Sì", "No");
            if (answer)
            {
                items.Clear(); // Cancella tutti gli elementi nella raccolta
                Debug.WriteLine("Operazione annullata ed elenco cancellato.");
            }
        }

        
        private void ConvertBtn_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("ConvertBtn_Clicked");
        }

        private async void DropGestureRecognizer_DragOver(object sender, DragEventArgs e)
        {
            pageService.OnDragOver(sender, e);
        }

        private void DropGestureRecognizer_DragLeave(object sender, DragEventArgs e)
        {
            Debug.WriteLine("DropGestureRecognizer_DragLeave");
        }

        private async void DropGestureRecognizer_Drop(object sender, DropEventArgs e)
        {
            // Ecco la chiave! Destruttura la tupla restituita da OnDrop
            var (newDroppedItems, messages) = await pageService.OnDrop(sender, e);

            foreach (var item in newDroppedItems)
            {
                // Aggiungere solo se l'elemento non esiste già nella raccolta
                if (!items.Contains(item))
                {
                    items.Add(item);
                }
            }

            // Mostra eventuali messaggi di errore/avviso
            if (messages.Any())
            {
                // Unisci tutti i messaggi in un'unica stringa
                string combinedMessages = string.Join("\n", messages);
                await Shell.Current.DisplayAlert("Avviso file", combinedMessages, "OK");
            }
        }
    }
}