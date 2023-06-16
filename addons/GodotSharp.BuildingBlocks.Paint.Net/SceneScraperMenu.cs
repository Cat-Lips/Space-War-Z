#if TOOLS
using Godot;

namespace GodotSharp.BuildingBlocks.Paint.Net
{
    [Tool]
    public partial class SceneScraperMenu
    {
        public Action Attach;
        public Action Detach;

        private string ScrapeSceneMenuItem { get; set; }

        public SceneScraperMenu(EditorPlugin plugin)
        {
            var editor = plugin.GetEditorInterface();
            foreach (var menu in editor.GetFileSystemDock().GetChildren<PopupMenu>())
            {
                Attach += () => menu.AboutToPopup += _OnMenuPopup;
                Detach += () => menu.AboutToPopup -= _OnMenuPopup;
                void _OnMenuPopup() => OnMenuPopup(menu);

                Attach += () => menu.IndexPressed += _OnMenuItemSelected;
                Detach += () => menu.IndexPressed -= _OnMenuItemSelected;
                void _OnMenuItemSelected(long index) => OnMenuItemSelected(menu, (int)index);
            }

            void OnMenuPopup(PopupMenu menu)
            {
                var path = editor.GetCurrentPath();
                if (SceneScraper.CanScrapeScene(path))
                {
                    menu.AddSeparator(" Scene Scraper ");
                    menu.AddItem(ScrapeSceneMenuItem = $"Scrape {path.GetDirName()}");
                }
            }

            void OnMenuItemSelected(PopupMenu menu, int index)
            {
                if (menu.GetItemText(index) == ScrapeSceneMenuItem)
                {
                    var path = editor.GetCurrentPath();

                    ShowProgress(out var progressDialog);
                    var content = SceneScraper.ExtractFiles(path, OnProgress);
                    CloseProgress();

                    SceneScraper.ImportFiles(editor, content);

                    ShowProgress(out progressDialog);
                    SceneScraper.ScrapeScene(path, OnProgress, content);
                    CloseProgress();

                    void ShowProgress(out ProgressDialog dlg)
                    {
                        dlg = App.InstantiateScene<ProgressDialog>();
                        editor.PopupDialogCentered(dlg);
                    }

                    void OnProgress(string msg, float progress)
                    {
                        progressDialog.Message = msg;
                        progressDialog.Progress = progress;
                        GD.Print($"{msg} ({progress:P0})");
                    }

                    void CloseProgress()
                    {
                        progressDialog.Hide();
                        progressDialog.Dispose();
                    }
                }
            }
        }
    }
}
#endif
