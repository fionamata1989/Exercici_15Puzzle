using CrystalDecisions.ReportAppServer.ReportDefModel;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Exercici_15puzzle
{
    /// <summary>
    /// Lógica de interacción para WndGraellaJoc.xaml
    /// </summary>
    public partial class WndGraellaJoc : Window
    {
        #region Graella
        private int nFiles;
        private int nColumnes;
        Grid graella;
        #endregion

        #region Cronòmetre
        DispatcherTimer tmrCronometre;
        TimeSpan tempsTranscorregut = TimeSpan.Zero;
        DateTime tempsInicial = DateTime.Now;
        bool tempsOn = false;
        #endregion

        #region Música
        MediaPlayer musica;
        String ruta = "C:\\Users\\fiona\\OneDrive\\Escritorio\\Exercici_15puzzle\\Exercici_15puzzle\\Musica\\SimsBuyMode1.wav";
        bool musicaOn = false;
        #endregion

        public WndGraellaJoc()
        {
            InitializeComponent();

            #region Creació Música
            musica = new MediaPlayer();
            musica.Open(new Uri(ruta));
            musica.Play();
            musicaOn = true;
            #endregion

            #region Creació Cronòmetre

            tmrCronometre = new DispatcherTimer();
            tmrCronometre.Interval = new TimeSpan(100);
            tmrCronometre.Tick += TmrCronometre_Tick;
            tmrCronometre.Start();
            tempsOn = true;
            
            #endregion
        }

        /// <summary>
        /// En carregar-se la finestra, carreguem la graella i les fitxes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wndGraellaJoc_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow finestraInici = (MainWindow)Owner;

            #region Creació Grid

            graella = new Grid();
            grdJoc.Children.Add(graella);
            nFiles = Convert.ToInt32(finestraInici.iudFiles.Text);
            nColumnes = Convert.ToInt32(finestraInici.iudColumnes.Text);

            graella.CreaTauler(nColumnes, nFiles);
            graella.ShowGridLines = true; //PER VEURE EL GRID
            graella.Margin = new Thickness(10);

            #endregion

            #region Creació de fitxes

            List<Fitxa> fitxes = new();
            int comptador = 1;
            for (int fila = 0; fila < nFiles; fila++)
            {
                for (int columna = 0; columna < nColumnes; columna++)
                {
                    Fitxa fitxa = new Fitxa();
                    //Aprofitem el .content per guardar el valor del comptador
                    fitxa.Content = comptador.ToString();
                    fitxa.Fila = fila;
                    fitxa.Columna = columna;
                    fitxa.OnApplyTemplate();
                    fitxes.Add(fitxa);
                    comptador++;
                }
            }
            //Borrem l'últim valor de la llista:
            fitxes.Remove(fitxes[fitxes.Count - 1]);

            //Desordenem la llista:
            fitxes.Desordena();

            //Emplenem el grid amb les fitxes:
            comptador = 0;
            for (int fila = 0; fila < nFiles; fila++)
            {
                for (int columna = 0; columna < nColumnes; columna++)
                {
                    //Controlem que no ens trobem a l'última posició del grid, que ha de quedar lliure:
                    if (!(fila == nFiles - 1 && columna == nColumnes - 1))
                    {
                        //Posicionem la fitxa
                        int numActual = Convert.ToInt32(fitxes[comptador].Content.ToString());
                        Fitxa fitxaNova = new (columna,fila);
                        fitxaNova.Content = fitxes[comptador].ToString();
                        Grid.SetRow(fitxaNova, fila);
                        Grid.SetColumn(fitxaNova, columna);
                        fitxaNova.Click += FitxaNova_Click;
                        graella.Children.Add(fitxaNova);
                        //PosicioCorrecta(fitxaNova);
                        comptador++;
                    }

                    //Posem la fitxa transparent al final:
                    else
                    {
                        Fitxa fitxaTransparent = new();
                        fitxaTransparent.Background = new SolidColorBrush(Color.FromRgb(236, 223, 233));
                        fitxaTransparent.Content = "";
                        Grid.SetRow(fitxaTransparent, nFiles-1);
                        Grid.SetColumn(fitxaTransparent, nColumnes-1);
                        graella.Children.Add(fitxaTransparent);
                        //Aprofitem el tag per guardar la posició de la fitxa transparent:
                        graella.Tag = fitxaTransparent;
                    }
                }
            }
            //Mirem si és soluble
            if (!EsSoluble(fitxes))
            {
                //Si no fos soluble, el fem soluble
                FerSoluble(fitxes);
            }
            Actualitza((WndGraellaJoc)wndGraellaJoc);
            #endregion

        }

        #region Funcions/Accions Joc

        /// <summary>
        /// Funció que gestiona el moviment de les fitxes. 
        /// En clicar en una d'elles, controlem què tenen de cantó, si una altra fitxa o la fitxa buida,
        /// i es posicionen en l'espai designat com a buit. Dins de la mateixa acció, també es controla 
        /// si la fitxa moguda es troba o no en la posició correcta i s'actualitzen les dades per poder 
        /// incrementar, disminuir o no el % de progrés. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FitxaNova_Click(object sender, RoutedEventArgs e)
        {
            Fitxa fitxaActual = (Fitxa)sender;
            Fitxa campBuit = (Fitxa)graella.Tag;
            int fila = Grid.GetRow(fitxaActual);
            int columna = Grid.GetColumn(fitxaActual);
            int filaCampBuit = Grid.GetRow(campBuit);
            int columnaCampBuit = Grid.GetColumn(campBuit);

            if (DeCostat(fitxaActual, campBuit))
            {
                Grid.SetRow(fitxaActual, filaCampBuit);
                Grid.SetColumn(fitxaActual, columnaCampBuit);
                
                Grid.SetRow(campBuit, fila);
                Grid.SetColumn(campBuit, columna);

                PosicioCorrecta(fitxaActual);
            }
            Actualitza(wndGraellaJoc);

        }

        /// <summary>
        /// Funció que controla la lògica de si la fitxa estàndard i la fitxa buida es troben o no de costat.
        /// És la lògica del moviment de la fitxa estàndard.
        /// </summary>
        /// <param name="fitxa"></param>
        /// <param name="fitxaBuida"></param>
        /// <returns>Retorna true si les dues fitxes estan de costat.</returns>
        private bool DeCostat(Fitxa fitxa, Fitxa fitxaBuida)
        {
            bool deCostat = false;

            int fila = Grid.GetRow(fitxa);
            int columna = Grid.GetColumn(fitxa);
            int filaForat = Grid.GetRow(fitxaBuida);
            int columnaForat = Grid.GetColumn(fitxaBuida);

            if (fila + 1 == filaForat && columna == columnaForat)
                deCostat = true;
            else if (fila - 1 == filaForat && columna == columnaForat)
                deCostat = true;
            else if (columna+1 == columnaForat && fila == filaForat)
                deCostat = true;
            else if (columna-1 == columnaForat && fila == filaForat)
                deCostat = true;

            return deCostat;
        }
        
        /// <summary>
        /// Funció booleana que controla si la fitxa es troba en la posició correcta i, en funció, 
        /// en canvia el color de fons i el color de la lletra al verd clar. 
        /// En cas contrari, es manté el color estàndard. 
        /// </summary>
        /// <param name="fitxa"></param>
        /// <returns>Retorna true si la posició coincideix amb la correcta.</returns>
        private bool PosicioCorrecta(Fitxa fitxa)
        {
            int filaActual = Grid.GetRow(fitxa);
            int columnaActual = Grid.GetColumn(fitxa);
            bool posicioCorrecta = false;

            if (fitxa.Valor.ToString() == (filaActual * nColumnes + columnaActual + 1).ToString())
            {
                posicioCorrecta = true;
                fitxa.Background = new SolidColorBrush(Color.FromRgb(211, 255, 206));
                fitxa.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            }
            else
            {
                fitxa.Background = new SolidColorBrush(Color.FromRgb(107, 89, 108));
                fitxa.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            }
            return posicioCorrecta;
        }

        /// <summary>
        /// Funció booleana que controla si la partida és o no soluble. Forma part de la lògica de funcionament del joc. 
        /// Conta els desordres que que hi ha en la llista de números (si el primer números és més gran que el que té al cantó). Si la suma dels desordres és parella, és soluble. En cas de ser senar, no és soluble. 
        /// </summary>
        /// <param name="fitxes"></param>
        /// <returns>Retorna true si la partida és soluble, és a dir, si la suma és parella.</returns>
        private bool EsSoluble(List<Fitxa> fitxes)
        {
            bool esSoluble = false;
            int desordresComptats = 0;
            int fitxaPrincipal;
            int fitxaAComparar = 0;

            for (int i = 0; i < fitxes.Count-2; i++)
            {
                fitxaPrincipal = Convert.ToInt32(fitxes[i].Content); 
                for (int n = i+1; n < fitxes.Count; n++)
                {
                    if (fitxaPrincipal > fitxaAComparar)
                    {
                        desordresComptats++;
                    }
                }
            }
            if (desordresComptats % 2 == 0)
                esSoluble = true;

            return esSoluble;
        }

        /// <summary>
        /// Acció que fa soluble a la partida intercanviant de lloc els dos últims valors de la llísta de fitxes. 
        /// </summary>
        /// <param name="fitxes"></param>
        private void FerSoluble (List<Fitxa> fitxes)
        {
            int i = 0;
            int ultimaFitxa;
            int penultimaFitxa;
            int aux;

            List<Fitxa> llistaInvertida = new List<Fitxa>(fitxes);
            llistaInvertida.Reverse();

            ultimaFitxa = Convert.ToInt32(llistaInvertida[i].Content);
            penultimaFitxa = Convert.ToInt32(llistaInvertida[i+1].Content); 

            aux = ultimaFitxa;
            ultimaFitxa = penultimaFitxa;
            penultimaFitxa = aux;

            llistaInvertida[i].Content = ultimaFitxa;
            llistaInvertida[i + 1].Content = penultimaFitxa;

            fitxes.RemoveAt(fitxes.Count - 2);
            fitxes.RemoveAt(fitxes.Count - 1);

            fitxes.Add(llistaInvertida[i+1]);
            fitxes.Add(llistaInvertida[i]);
        }
        
        /// <summary>
        /// Ácció que s'encarrega de calcular quantes fitxes es troben en la posició correcta i el % de partida solucionada.
        /// Ha d'anar en qualsevol posició del programa després de fer un moviment de fitxa.
        /// </summary>
        /// <param name="finestraJoc"></param>
        private void Actualitza(WndGraellaJoc finestraJoc)
        {
            double posicioCorrecta = 0, nTotal = 0;

            foreach (Fitxa fitxa in finestraJoc.graella.Children)
            {
                nTotal++;
                if (fitxa.Content == "")
                {
                    fitxa.Background = new SolidColorBrush(Colors.Transparent);
                    
                }
                else if (PosicioCorrecta(fitxa))
                {
                    posicioCorrecta++;
                }
            }
            nTotal--; //Restem la fitxa buida/transparent/forat
            int percentatge = Convert.ToInt32(Math.Round(posicioCorrecta * 100) / (finestraJoc.nFiles * finestraJoc.nColumnes));
            finestraJoc.tbComptadorClic.Text = "Completat: " + percentatge.ToString() + "%";
        }
        #endregion

        #region Funcion/Accions Cronometre

        /// <summary>
        /// Crea, calcuka i nostra el temps.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TmrCronometre_Tick(object? sender, EventArgs e)
        {
            tempsTranscorregut = DateTime.Now.Subtract(tempsInicial);
            MostraTemps(tbTemps, tempsTranscorregut);
        }

        /// <summary>
        /// Indica com cal mostrar el temps, en el nostre cas, és en format hh:mm:ss.
        /// </summary>
        /// <param name="textBlock"></param>
        /// <param name="temps"></param>
        private void MostraTemps(TextBlock textBlock, TimeSpan temps)
        {
            textBlock.Text = $"{tempsTranscorregut.Hours:00}:" +
                $"{tempsTranscorregut.Minutes:00}:" +
                $"{tempsTranscorregut.Seconds:00}";
        }

        /// <summary>
        /// Acció que gestiona la pausa del temps i que amaga la graella de joc de la vis´ta de l'usuari.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPausa_Click(object sender, RoutedEventArgs e)
        {
            Pausa();
        }

        /// <summary>
        /// Acció que permet pausar la partida. Aïllada per poder fer-la anar tant des de la tecla P com des del botó "Pausar". 
        /// </summary>
        private void Pausa()
        {
            if (tempsOn)
            {
                tmrCronometre.Stop();
                btnPausa.Content = "Reprendre";
                grdJoc.Visibility = Visibility.Collapsed;
            }
            else
            {
                tempsInicial = DateTime.Now - tempsTranscorregut;
                btnPausa.Content = "Pausar";
                grdJoc.Visibility = Visibility.Visible;
                tmrCronometre.Start();
            }
            tempsOn = !tempsOn;
        }
        private void wndGraellaJoc_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.P)
            {
                Pausa();
            }
        }
        #endregion

        #region Funcions/Accions Música
        /// <summary>
        /// Acció destinada a apagar o encendre la música, depenent de l'estat. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMusica_Click(object sender, RoutedEventArgs e)
        {
            if (musicaOn)
            {
                musica.Pause();
                btnMusica.Content = "Engegar música";
            }
            else
            {
                musica.Play();
                btnMusica.Content = "Apagar música";
            }
            musicaOn = !musicaOn;
        }

        #endregion


    }

    #region Mètodes d'extensió

    public static class MetodesExtensio
    {
        /// <summary>
        /// Creació de la graella del joc en funció de les files i columnes indicades. 
        /// </summary>
        /// <param name="graellaJoc"></param>
        /// <param name="nColumnes"></param>
        /// <param name="nFiles"></param>
        public static void CreaTauler(this Grid graellaJoc, int nColumnes, int nFiles)
        {
            CreaFiles(graellaJoc, nFiles);
            CreaColumnes(graellaJoc, nColumnes);
        }

        /// <summary>
        /// Creació de les columnes a la graella del joc segons les indicacions prèvies donades per l'usuari. 
        /// </summary>
        /// <param name="graellaJoc"></param>
        /// <param name="nColumnes"></param>
        private static void CreaColumnes(Grid graellaJoc, int nColumnes)
        {
            for (int columna = 0; columna < nColumnes; columna++)
            {
                ColumnDefinition cd = new ColumnDefinition();
                cd.Width = new GridLength(1, GridUnitType.Star);
                graellaJoc.ColumnDefinitions.Add(cd);
            }
        }

        /// <summary>
        /// Creació de les files a la graella del joc segons les indicacions prèvies donades per l'usuari. 
        /// </summary>
        /// <param name="graellaJoc"></param>
        /// <param name="nFiles"></param>
        private static void CreaFiles(Grid graellaJoc, int nFiles)
        {
            for (int fila = 0; fila < nFiles; fila++)
            {
                RowDefinition rd = new RowDefinition();
                //Fent la nova gridDefinition, posarie, això per 
                rd.Height = new GridLength(1F, GridUnitType.Star);
                graellaJoc.RowDefinitions.Add(rd);
            }
        }

        /// <summary>
        /// Acció que desordena les fitxes del taulell de joc.
        /// </summary>
        /// <param name="llistaFitxes"></param>
        public static void Desordena(this List<Fitxa> llistaFitxes)
        {
            Random random = new Random();
            int n = llistaFitxes.Count;
            
            while (n>1)
            {
                n--;
                int k = random.Next(n + 1);
                Fitxa value = llistaFitxes[k];
                llistaFitxes[k] = llistaFitxes[n];
                llistaFitxes[n] = value;
            }
        }
    }

    #endregion
}
