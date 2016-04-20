using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Text.RegularExpressions;

namespace Molecular_Weighter
{
    public partial class MainWindow : Window
    {
        static List<Atom> atoms = new List<Atom>();

        public MainWindow()
        {
            InitializeComponent();

            LoadData();

            var t = GetContentsOfMoleculaByFormula("(H2Cl)3Na(S2O4)3");

            //var t = GetAtomicWeightByFormula("H2SO4");
            //var t = GetContentsOfMoleculaByFormula("H2SO4");
            //;
        }

        //static double GetAtomicWeightByFormula(string formula)
        //{
        //    var formulaRegex = $@"(?<Symbol>{string.Join("|", atoms.Select(a => a.Symbol).ToArray())})(?<Count>\d*)";

        //    double totalMass = 0;
        //    var matches = Regex.Matches(formula, formulaRegex);

        //    foreach(Match atom in matches)
        //    {
        //        var symbol = atom.Groups["Symbol"].Value;
        //        var count = atom.Groups["Count"].Value == ""? 1 : int.Parse(atom.Groups["Count"].Value);

        //        var atomicMass = atoms.Where(a => a.Symbol == symbol).First().AtomicWeight;

        //        totalMass += count * atomicMass;
        //    }

        //    return totalMass;
        //}


        static string formulaRegex;
        static string wholeMoleculaRegex;
        static Dictionary<string, int> GetContentsOfMoleculaByFormula(string formula)
        {
            var contentOfMolecula = new Dictionary<string, int>();

            var matches = Regex.Matches(formula, formulaRegex);

            foreach (Match atom in matches)
            {
                var symbol = atom.Groups["Symbol"].Value;
                var count = atom.Groups["Count"].Value == "" ? 1 : int.Parse(atom.Groups["Count"].Value);

                if (contentOfMolecula.ContainsKey(symbol))
                    contentOfMolecula[symbol] += count;
                else
                    contentOfMolecula[symbol] = count;

                //var atomicMass = atoms.Where(a => a.Symbol == symbol).First().AtomicWeight;

                    //totalMass += count * atomicMass;
            }

            return contentOfMolecula;
        }




        static void LoadData()
        {
            var datafileContent = System.IO.File.ReadAllText("Data.txt", Encoding.Default);

            var dataRegex = @"(?<AtomicNumber>\d+)\s+(?<Symbol>.+?)\s+(?<Name>.+?)\s+(?<AtomicWeight>.+?)\n";

            var matches = Regex.Matches(datafileContent, dataRegex);

            foreach(Match match in matches)
            {
                var atomicNumber = int.Parse(match.Groups["AtomicNumber"].Value);
                var symbol = match.Groups["Symbol"].Value;
                var name = match.Groups["Name"].Value;
                var atomicWeight = double.Parse(match.Groups["AtomicWeight"].Value.Replace('.', ','));

                atoms.Add(new Atom
                {
                    AtomicNumber = atomicNumber,
                    Symbol = symbol,
                    Name = name,
                    AtomicWeight = atomicWeight
                });
            }

            formulaRegex = $@"(?<Symbol>{string.Join("|", atoms.Select(a => a.Symbol).OrderByDescending(s => s.Length).ToArray())})(?<Count>\d*)";
            wholeMoleculaRegex = $"(({formulaRegex})+)";
        }

        private void Formula_textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (!Regex.IsMatch(Formula_textBox.Text, wholeMoleculaRegex))
            if (Formula_textBox.Text == "")
            {
                Results_textBox.Text = "";

                return;
            }

            var t = Regex.Match(Formula_textBox.Text, wholeMoleculaRegex).Value;

            if (Regex.Match(Formula_textBox.Text, wholeMoleculaRegex).Value != Formula_textBox.Text)
            {
                Results_textBox.Text = "Формула содержит ошибки.";

                return;
            }

            var molecula = GetContentsOfMoleculaByFormula(Formula_textBox.Text);

            //var header = string.Format("{0}", "Элемент");
            var totalMass = 0.0;
            var header = $"{"Элемент"}{"Атомная масса",25}{"Количество",15}{"Масса, всего",15}" + Environment.NewLine;

            foreach(var atom in molecula)
            {
                var currentAtom = atoms.Where(a => a.Symbol == atom.Key).First();

                var atomsMass = atom.Value * currentAtom.AtomicWeight;
                totalMass += atomsMass;

                var firstColumn = $"{currentAtom.Symbol + " (" + currentAtom.Name + ")"}";
                var secondColum = currentAtom.AtomicWeight.ToString();
                var thirdColumn = atom.Value.ToString();
                var fourthColumn = atomsMass.ToString();

                //var lineFormat = "{0}{1," + firstColumn.Length + secondColum.Length +  "}{2,20}{3,20}";
                //var lineFormat = "{0}{1,20}{2,15}{3,15}";
                var lineFormat = "{0}{1}{2}{3}";

                header += string.Format(lineFormat, firstColumn.PadRight(22, ' '), secondColum.PadRight(19, ' '), thirdColumn.PadRight(12, ' '), atomsMass) + Environment.NewLine;

                //var width = 30 - firstColumn.Length;

                //header += $"{firstColumn}{currentAtom.AtomicWeight,20}{atom.Value,15}{atomsMass,15}" + Environment.NewLine;
            }

            header += Environment.NewLine;
            header += $"Атомная масса {Regex.Match(Formula_textBox.Text, wholeMoleculaRegex).Value} = {totalMass}";

            Results_textBox.Text = header;
        }
    }

    class Atom
    {
        public int AtomicNumber { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public double AtomicWeight { get; set; }
    }
}
