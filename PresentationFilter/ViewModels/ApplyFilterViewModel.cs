using Autodesk.Revit.DB;
using PresentationFilter.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PresentationFilter.ViewModels
{
    public class ApplyFilterViewModel : BindableBase
    {
        private TransactionGroup _transactionGroup;
        private Document _document;
        private FillPatternElement _fillPattern;
        private ObservableCollection<View> _viewTemplate;
        public ObservableCollection<View> ViewTemplate
        {
            get { return _viewTemplate; }
            set { SetProperty(ref _viewTemplate, value); }
        }

        private View _SelectedViewTemplate;
        public View SelectedViewTemplate
        {
            get { return _SelectedViewTemplate; }
            set { SetProperty(ref _SelectedViewTemplate, value); }
        }
        private string _FilterNameBeginWith = "梁レベル/Structual Framing Level_FL";
        public string FilterNameBeginWith
        {
            get { return _FilterNameBeginWith; }
            set { SetProperty(ref _FilterNameBeginWith, value); }
        }


        private ObservableCollection<ParameterFilterElement> _ParameterFilterElement;
        public ObservableCollection<ParameterFilterElement> ParameterFilterElement
        {
            get { return _ParameterFilterElement; }
            set { SetProperty(ref _ParameterFilterElement, value); }
        }



        public ICommand ApplyFilters { get; private set; }

        public ApplyFilterViewModel()
        {
            _document = DIContainer.Instance.Resolve<Document>();
            _transactionGroup = DIContainer.Instance.Resolve<TransactionGroup>();
            ViewTemplate = GetViewTemplates(_document);
            SelectedViewTemplate = ViewTemplate[0];
            ParameterFilterElement = GetParameterFilterElements(_document);
            _fillPattern = GetFillPatternElements(_document);



            ApplyFilters = new DelegateCommand(() =>
            {
                if (!_transactionGroup.HasStarted())
                {
                    return;
                }
                using (Transaction transaction = new Transaction(_document, "Create View Filter"))
                {
                    transaction.Start();
                    Color[] basicColors = new Color[]
                        {
                            new Color(255, 0, 0),   // Red
                            new Color(255, 165, 0), // Orange
                            new Color(255, 255, 0), // Yellow
                            new Color(0, 255, 0),   // Green
                            new Color(0, 0, 255),   // Blue
                            new Color(128, 0, 128)  // Purple
                            // Thêm màu cơ bản khác nếu cần
                        };

                    List<Color> allColors = new List<Color>();


                    // Lặp lại mảng màu cơ bản nhiều lần để đủ 50 màu
                    for (int i = 0; i < 50; i++)
                    {
                        allColors.AddRange(basicColors);
                    }

                    int colorIndex = 0;

                    Random random = new Random();
                    HashSet<Color> usedColors = new HashSet<Color>();

                    double reductionFactor = 0.7; // Giảm 30% độ đậm
                    foreach (var item in ParameterFilterElement)
                    {
                        Color currentColor = allColors[colorIndex % allColors.Count];
                        Color reducedColor = ReduceSaturation(currentColor, reductionFactor);

                        Filter(SelectedViewTemplate, item, _fillPattern, reducedColor);

                        colorIndex++;
                    }

                    // Function to convert HSL to RGB

                    MessageBox.Show("Success");
                    transaction.Commit();
                }
                _transactionGroup.Commit();
            });

        }

        private ObservableCollection<View> GetViewTemplates(Document doc)
        {
            var viewTemplates = new FilteredElementCollector(doc).OfClass(typeof(View)).Cast<View>().Where(v => v.IsTemplate).ToList();
            return new ObservableCollection<View>(viewTemplates.OrderBy(v => v.Name));
        }

        private ObservableCollection<ParameterFilterElement> GetParameterFilterElements(Document doc)
        {
            var viewTemplates = new FilteredElementCollector(doc).OfClass(typeof(ParameterFilterElement)).Cast<ParameterFilterElement>().Where(v => v.Name.StartsWith(FilterNameBeginWith)).ToList();
            return new ObservableCollection<ParameterFilterElement>(viewTemplates);
        }
        private FillPatternElement GetFillPatternElements(Document doc)
        {
            return new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement)).Cast<FillPatternElement>().FirstOrDefault(v => v.Name.Equals("<Solid fill>"));
        }
        private void Filter(View view, ParameterFilterElement paramFilter, FillPatternElement fillPattern, Color color)
        {

            OverrideGraphicSettings overrideGraphicSettings = new OverrideGraphicSettings();
            overrideGraphicSettings.SetSurfaceForegroundPatternId(fillPattern.Id);

            overrideGraphicSettings.SetSurfaceForegroundPatternColor(color);


            view.AddFilter(paramFilter.Id);
            view.SetFilterOverrides(paramFilter.Id, overrideGraphicSettings);
        }

        private Color ReduceSaturation(Color color, double reductionFactor)
        {
            double hue, saturation, lightness;

            // Chuyển đổi màu RGB sang HSL
            RGBToHSL(color, out hue, out saturation, out lightness);

            // Giảm độ đậm
            saturation *= reductionFactor;

            // Chuyển đổi lại từ HSL sang RGB
            return HSLToRGB(hue, saturation, lightness);
        }

        private void RGBToHSL(Color color, out double hue, out double saturation, out double lightness)
        {
            double r = color.Red / 255.0;
            double g = color.Green / 255.0;
            double b = color.Blue / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));

            double delta = max - min;

            lightness = (max + min) / 2.0;

            if (delta == 0)
            {
                hue = 0;
                saturation = 0;
            }
            else
            {
                saturation = lightness > 0.5 ? delta / (2.0 - max - min) : delta / (max + min);

                if (max == r)
                    hue = ((g - b) / delta + (g < b ? 6 : 0)) / 6.0;
                else if (max == g)
                    hue = ((b - r) / delta + 2) / 6.0;
                else
                    hue = ((r - g) / delta + 4) / 6.0;
            }

            hue = (hue * 360.0 + 360.0) % 360.0;
            saturation *= 100;
            lightness *= 100;
        }

        private Color HSLToRGB(double hue, double saturation, double lightness)
        {
            hue /= 360.0;
            saturation /= 100.0;
            lightness /= 100.0;

            double r, g, b;

            if (saturation == 0)
            {
                r = g = b = lightness; // achromatic
            }
            else
            {
                double q = lightness < 0.5 ? lightness * (1.0 + saturation) : lightness + saturation - lightness * saturation;
                double p = 2.0 * lightness - q;

                r = HueToRGB(p, q, hue + 1.0 / 3.0);
                g = HueToRGB(p, q, hue);
                b = HueToRGB(p, q, hue - 1.0 / 3.0);
            }

            return new Color((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }

        private double HueToRGB(double p, double q, double t)
        {
            if (t < 0.0) t += 1.0;
            if (t > 1.0) t -= 1.0;
            if (t < 1.0 / 6.0) return p + (q - p) * 6.0 * t;
            if (t < 1.0 / 2.0) return q;
            if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6.0;
            return p;
        }


    }
}
