using Microsoft.Extensions.Logging; // Aggiunto per il metodo .Any()
using QuestPDF.Fluent;
using System.Diagnostics;

namespace WG.PdfTools.Pages
{
    public class MainPageService
    {
        private readonly ILogger<MainPageService> _logger;

        public MainPageService(ILogger<MainPageService> logger)
        {
            _logger = logger;
        }
        // Abbiamo modificato il tipo di ritorno per includere messaggi di errore
        public async Task<(List<string> PdfFiles, List<string> Messages)> OnDrop(object sender, DropEventArgs e)
        {
            List<string> pdfFiles = new List<string>();
            List<string> messages = new List<string>();

#if WINDOWS // Si applica solo alla piattaforma Windows, se ti trovi su un'altra piattaforma, questo viene ignorato
            var windowsDragEventArgs = e.PlatformArgs?.DragEventArgs;
            if (windowsDragEventArgs is null)
            {
                Debug.WriteLine("Drop senza PlatformArgs.");
                messages.Add("L'azione di rilascio non può essere elaborata (nessun argomento di piattaforma).");
                return (pdfFiles, messages);
            }
            var windowsDragUI = windowsDragEventArgs.DragUIOverride;

            if (windowsDragUI is null)
            {
                Debug.WriteLine("Windows Drop senza DragUIOverride.");
                messages.Add("L'azione di rilascio non è stata elaborata (senza DragUIOverride).");
                return (pdfFiles, messages);
            }

            var draggedOverItems = await windowsDragEventArgs.DataView.GetStorageItemsAsync();
            if (draggedOverItems is null)
            {
                Debug.WriteLine("Windows Drop: Non sono stati ottenuti elementi trascinati.");
                messages.Add("Non sono stati rilevati file da eliminare.");
                return (pdfFiles, messages);
            }

            if (draggedOverItems.Any())
            {
                Debug.WriteLine($"Drop con {draggedOverItems.Count()} elementos.");
                windowsDragUI.Caption = "Rilascia per copiare"; // Messaggio predefinito
                windowsDragUI.IsContentVisible = true;
                windowsDragUI.IsGlyphVisible = true;

                foreach (var item in draggedOverItems)
                {
                    if (item is Windows.Storage.StorageFile file)
                    {
                        var fileExt = file.FileType.ToLower();
                        if (fileExt == ".pdf")
                        {
                            pdfFiles.Add(file.Path);
                            Debug.WriteLine($"Archivio {file.Path} rilasciato.");
                        }
                        else
                        {
                            messages.Add($"'{file.Name}' Non è un file PDF ed è stato ignorato.");
                            Debug.WriteLine($"Archivo {file.Path} Non è un PDF. Ignorato.");
                        }
                    }
                }
            }
            else
            {
                Debug.WriteLine("Drop senza elementi.");
                messages.Add("Non sono stati rilevati file da eliminare.");
            }
#endif
            return (pdfFiles, messages);
        }

        public async void OnDragOver(object sender, DragEventArgs e)
        {
#if WINDOWS
            var windowsDragEventArgs = e.PlatformArgs?.DragEventArgs;

            if (windowsDragEventArgs is null)
            {
                Debug.WriteLine("DragOver senza PlatformArgs.");
                e.AcceptedOperation = DataPackageOperation.None;
                return;
            }

            var windowsDragUI = windowsDragEventArgs.DragUIOverride;

            if (windowsDragUI is null)
            {
                Debug.WriteLine("Windows DragOver senza DragUIOverride.");
                // Non possiamo modificare l'interfaccia utente se non c'è DragUIOverride, ma possiamo comunque controllare e.AcceptedOperation
                return;
            }

            var draggedOverItems = await windowsDragEventArgs.DataView.GetStorageItemsAsync();
            if (draggedOverItems is null)
            {
                Debug.WriteLine("Windows DragOver: Non sono stati ottenuti elementi trascinati.");
                e.AcceptedOperation = DataPackageOperation.None;
                windowsDragUI.IsContentVisible = false;
                windowsDragUI.IsGlyphVisible = false;
                return;
            }

            bool allPdf = true;
            if (draggedOverItems.Any())
            {
                foreach (var item in draggedOverItems)
                {
                    if (item is Windows.Storage.StorageFile file)
                    {
                        var fileExt = file.FileType.ToLower();
                        if (fileExt != ".pdf")
                        {
                            allPdf = false;
                            break; // Se ne troviamo uno che non è un PDF, sappiamo già che non tutti lo sono
                        }
                    }
                }

                if (allPdf)
                {
                    e.AcceptedOperation = DataPackageOperation.Copy;
                    windowsDragUI.Caption = "Rilascia qui i file PDF";
                    windowsDragUI.IsContentVisible = true;
                    windowsDragUI.IsGlyphVisible = true;
                    Debug.WriteLine($"DragOver con {draggedOverItems.Count()} file PDF.");
                }
                else
                {
                    e.AcceptedOperation = DataPackageOperation.None;// Non consentire l'eliminazione se sono presenti file non PDF
                    windowsDragUI.Caption = "Sono consentiti solo i file PDF";
                    windowsDragUI.IsContentVisible = true;
                    windowsDragUI.IsGlyphVisible = false; // Nascondi il glifo di copia se non consentito
                    Debug.WriteLine($"DragOver con elementi non PDF. Operazione negata.");
                }
            }
            else
            {
                Debug.WriteLine("DragOver senza elementi.");
                e.AcceptedOperation = DataPackageOperation.None;
                windowsDragUI.IsContentVisible = false;
                windowsDragUI.IsGlyphVisible = false;
            }
#endif
        }

        public bool MergeFiles(List<string> files, string output)
        {
            //try
            //{
                if (files is null || files.Count < 2) return false;

                var documentOperator = DocumentOperation.LoadFile(files[0]);
                for (int i = 1; i < files.Count; i++)
                {
                    documentOperator = documentOperator.MergeFile(files[i]);
                }
                documentOperator.Save(output);
                return true;
            //}
            //catch (Exception e)
            //{
            //    _logger.LogError("Errore durante la fusione dei file PDF: {Message}", e.Message);   
            //    return false;
            //}
        }
    }
}