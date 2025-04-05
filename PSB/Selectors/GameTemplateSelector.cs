using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PSB.Interfaces;
using PSB.Models;

namespace PSB.Selectors
{
    public partial class GameTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? MainGameTemplate { get; set; }
        public DataTemplate? SideGameTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is Library library)
            {
                if (library.SideGame != null)
                    return SideGameTemplate!;
                else if (library.Game != null)
                    return MainGameTemplate!;
            }
            if(item is IGame game)
            {
                if(game.Type == "sideGame")
                    return SideGameTemplate!;
                else if(game.Type == "game")
                    return MainGameTemplate!;
            }

            return base.SelectTemplateCore(item, container);
        }
    }
}
