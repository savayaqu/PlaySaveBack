using System;
using System.Diagnostics;
using HtmlAgilityPack;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Text;

namespace PSB.Converters
{
    public static class HtmlToXamlConverter
    {
        public static RichTextBlock ConvertHtmlToXaml(string html)
        {
            var richTextBlock = new RichTextBlock();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            Debug.WriteLine($"Parsed HTML: {html}");

            foreach (var node in doc.DocumentNode.ChildNodes)
            {
                var paragraph = new Paragraph();
                ProcessNode(node, paragraph);
                if (paragraph.Inlines.Count > 0)
                {
                    richTextBlock.Blocks.Add(paragraph);
                }
            }

            return richTextBlock;
        }

        private static void ProcessNode(HtmlNode node, Paragraph paragraph)
        {
            if (node == null) return;

            // Если это <img>, сразу добавляем изображение
            if (node.Name == "img")
            {
                AddImage(node, paragraph);
            }
            else
            {
                // Для всех других узлов проверяем дочерние элементы
                foreach (var childNode in node.ChildNodes)
                {
                    ProcessNode(childNode, paragraph); // Обрабатываем дочерние узлы
                }

                // Обрабатываем текущий узел, если это текст или другой тег
                switch (node.Name)
                {
                    case "p":
                        AddParagraph(node, paragraph);
                        break;

                    case "h1":
                    case "h2":
                        AddHeader(node, paragraph, node.Name);
                        break;

                    case "b":
                    case "strong":
                        AddBold(node, paragraph);
                        break;

                    case "i":
                    case "em":
                        AddItalic(node, paragraph);
                        break;

                    case "br":
                        paragraph.Inlines.Add(new LineBreak());
                        break;

                    default:
                        // Для других тегов можно ничего не делать, или добавлять их как текст
                        break;
                }
            }
        }

        private static void AddImage(HtmlNode node, Paragraph paragraph)
        {
            var imgSrc = node.GetAttributeValue("src", string.Empty).Trim();
            Debug.WriteLine($"Found img tag with src: '{imgSrc}'");

            if (!string.IsNullOrEmpty(imgSrc))
            {
                var image = new Image
                {
                    Source = new BitmapImage(new Uri(imgSrc)),
                    //Margin = new Microsoft.UI.Xaml.Thickness(5) // Пространство вокруг изображения
                };

                var inlineUIContainer = new InlineUIContainer
                {
                    Child = image
                };
                paragraph.Inlines.Add(inlineUIContainer);
            }
        }



        private static void AddParagraph(HtmlNode node, Paragraph paragraph)
        {
            var text = node.InnerText.Trim();
            if (!string.IsNullOrEmpty(text))
            {
                paragraph.Inlines.Add(new Run { Text = text + "\n\n" });
            }
        }

        private static void AddHeader(HtmlNode node, Paragraph paragraph, string tag)
        {
            var headerRun = new Run
            {
                Text = node.InnerText.Trim() + "\n\n",
                FontSize = tag == "h1" ? 24 : 20,
                FontWeight = FontWeights.Bold
            };
            paragraph.Inlines.Add(headerRun);
        }

        private static void AddBold(HtmlNode node, Paragraph paragraph)
        {
            var bold = new Bold();
            foreach (var child in node.ChildNodes)
            {
                bold.Inlines.Add(new Run { Text = child.InnerText.Trim() });
            }
            paragraph.Inlines.Add(bold);
        }

        private static void AddItalic(HtmlNode node, Paragraph paragraph)
        {
            var italic = new Italic();
            foreach (var child in node.ChildNodes)
            {
                italic.Inlines.Add(new Run { Text = child.InnerText.Trim() });
            }
            paragraph.Inlines.Add(italic);
        }

        
    }
}
