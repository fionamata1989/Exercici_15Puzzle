using CrystalDecisions.ReportAppServer.CommonControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Exercici_15puzzle
{
    public class Fitxa : Button
    {

        #region propietats
        public int? Fila { get; set; }
        public int? Columna { get; set; }
        public Viewbox Vb { get; set; }
        public TextBlock Tb { get; set; }
        public int Valor { get; set; }

        //Per poder fer el canvi de color
        public bool? PosicioCorrecta { get; set; }

        #endregion

        public Fitxa()
        {
            Viewbox viewbox = new Viewbox();
            TextBlock textBlock = new TextBlock();
            Content = viewbox;
            textBlock.Text = Content.ToString();
            Style = (Style)Application.Current.FindResource("FitxaStyle");

        }

        public Fitxa(int columna, int fila): this() 
        {
            Fila = fila;
            Columna = columna;
        }

        public override string ToString()
        {   
            return Content.ToString();
        }

       
    }
}
