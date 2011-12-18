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
using System.Windows.Shapes;

//kinect
using Microsoft.Research.Kinect.Nui;
using Coding4Fun.Kinect.Wpf;
using System.Threading;

//media
using System.Media;
using System.Windows.Media.Animation;


//speech recognition
using Microsoft.Research.Kinect.Audio;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System.IO;

namespace countdown_bug_testing
{
    /// <summary>
    /// Interaction logic for WindowG.xaml
    /// </summary>
    public partial class WindowMenu : Window
    {

        Runtime nui;
        
        RadioButton[] radiolist = new RadioButton[4];
        TextBlock[] letterslist = new TextBlock[26];

        string[] DisplayName = new string[3];
        int DisplayNameIndex = 0;

        MainWindow mw;

        bool gameRunning = false;
        bool lastpunch = true;
        bool handpunch = true;
        int points = 0;

        SoundPlayer menu_sound;

        HighScore hss = new HighScore();

        //Speech recognition Global vars
        private Thread t;
        private const string RecognizerId = "SR_MS_en-US_Kinect_10.0";

        private Choices words;

        private KinectAudioSource source;
        private SpeechRecognitionEngine sre;
        private Stream stream;

        private bool order_vocal = false;

        //--------- end of speech global vars

        public WindowMenu()
        {
            InitializeComponent();

            //thread for speech rec
            t = new System.Threading.Thread(
              new System.Threading.ThreadStart(
                delegate()
                {
                    debugSpeech.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                      delegate()
                      {
                          CaptureAudio();
                      }
                  ));
                }
            ));

            t.SetApartmentState(ApartmentState.MTA);
            t.Start();

        }


        private void InitVars() /* hardcode values here*/
        {

            menu_sound = new SoundPlayer("menu_select2.wav");


            words = new Choices();

            words.Add("computer");
            words.Add("start");
            words.Add("pause"); 
            words.Add("pause game");
            words.Add("resume");
            words.Add("resume battle");
            words.Add("exit");
            words.Add("quit");
            words.Add("please exit");
            words.Add("please quit");


            DisplayName[0] = "_";
            DisplayName[1] = "_";
            DisplayName[2] = "_";

            radiolist[0] = radioEasy;
            radiolist[1] = radioMedium;
            radiolist[2] = radioHard;
            radiolist[3] = radioInsane;

            letterslist[0] = textBlock1;
            letterslist[1] = textBlock2;
            letterslist[2] = textBlock3;
            letterslist[3] = textBlock4;
            letterslist[4] = textBlock5;
            letterslist[5] = textBlock6;
            letterslist[6] = textBlock7;
            letterslist[7] = textBlock8;
            letterslist[8] = textBlock9;
            letterslist[9] = textBlock10;
            letterslist[10] = textBlock11;
            letterslist[11] = textBlock12;
            letterslist[12] = textBlock13;
            letterslist[13] = textBlock14;
            letterslist[14] = textBlock15;
            letterslist[15] = textBlock16;
            letterslist[16] = textBlock17;
            letterslist[17] = textBlock18;
            letterslist[18] = textBlock19;
            letterslist[19] = textBlock20;
            letterslist[20] = textBlock21;
            letterslist[21] = textBlock22;
            letterslist[22] = textBlock23;
            letterslist[23] = textBlock24;
            letterslist[24] = textBlock25;
            letterslist[25] = textBlock26;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            SetupKinect();
            InitVars();

        }


        #region Speech recognituiobn

        private void CaptureAudio()
        {

            this.source = new KinectAudioSource();

            this.source.FeatureMode = true;
            this.source.AutomaticGainControl = false;
            this.source.SystemMode = SystemMode.OptibeamArrayOnly;

            RecognizerInfo ri = SpeechRecognitionEngine.InstalledRecognizers().Where(r => r.Id == RecognizerId).FirstOrDefault();

            if (ri == null)
            {
                MessageBox.Show("Cannot load Speech recognition");
                return;
            }

            this.sre = new SpeechRecognitionEngine(ri.Id);

            var gb = new GrammarBuilder();

            gb.Culture = ri.Culture;

            gb.Append(words);

            var g = new Grammar(gb);

            sre.LoadGrammar(g);

            this.sre.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(sre_SpeechRecognized);

            this.sre.SpeechHypothesized += new EventHandler<SpeechHypothesizedEventArgs>(sre_SpeechHypothesized);

            this.sre.SpeechRecognitionRejected += new EventHandler<SpeechRecognitionRejectedEventArgs>(sre_SpeechRecognitionRejected);

            this.stream = this.source.Start();

            this.sre.SetInputToAudioStream(this.stream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));

            this.sre.RecognizeAsync(RecognizeMode.Multiple);

        }


        private void sre_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {

            //this.WordRejected = true;

        }

        private void sre_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            //MessageBox.Show("Hypothesized: " + e.Result.Text + " with confidence " + e.Result.Confidence);
            //this.HypothesizedWord = e.Result.Text;

        }

        private void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            debugSpeech.Text = "Recognized: " + e.Result.Text;

            if (order_vocal)
            {

                switch (e.Result.Text)
                {
                    case "start":
                        {
                            startgame();
                            break;
                        }

                    case "please quit":
                        {
                            this.Close();
                            break;
                        }

                        
                    case "pause game":
                        {
                            if (gameRunning)
                                mw.pausefromMenu();
                            break;
                        }
                        


                    case "resume battle":
                        {
                            if (gameRunning)
                                mw.startFromMenu();
                            break;
                        }


                    case "please exit":
                        {
                            if (gameRunning)
                                mw.Close();
                            break;
                        }
                }
            }



            order_vocal = (e.Result.Text == "computer");

        }


        #endregion


        private void SetupKinect()
        {
         
            if (Runtime.Kinects.Count == 0)
            {
                this.Title = "No Kinect connected";
            }
            else
            {
                //use first Kinect
                nui = Runtime.Kinects[0];

                //Initialize to do skeletal tracking
                nui.Initialize(RuntimeOptions.UseSkeletalTracking | RuntimeOptions.UseColor );

                //add event to receive skeleton data
                nui.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);

                nui.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
       
                
                nui.SkeletonEngine.TransformSmooth = true;
                TransformSmoothParameters parameters = new TransformSmoothParameters();
                parameters.Smoothing = 0.3f;
                parameters.Correction = 0.9f;
                parameters.Prediction = 0.9f;
                parameters.JitterRadius = 1.0f;
                parameters.MaxDeviationRadius = 0.5f;
                nui.SkeletonEngine.SmoothParameters = parameters;
                

            }

        }

        
        void nui_ColorFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
        }


        void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {

            if (gameRunning)
                return;

            SkeletonFrame allSkeletons = e.SkeletonFrame;

            //get the first tracked skeleton
            SkeletonData skeleton = (from s in allSkeletons.Skeletons
                                     where s.TrackingState == SkeletonTrackingState.Tracked
                                     select s).FirstOrDefault();



            if (skeleton != null)
            {

                debugTrack.Text = "Tracking";

                /*
                var scaledJointLeft = skeleton.Joints[JointID.HandLeft].ScaleTo(640, 480, .8f, .8f);
                var scaledJointRight = skeleton.Joints[JointID.HandRight].ScaleTo(640, 480, .8f, .8f);
                */

                var scaledJointLeft = skeleton.Joints[JointID.HandLeft].ScaleTo(890, 540, .8f, .8f);
                var scaledJointRight = skeleton.Joints[JointID.HandRight].ScaleTo(890, 540, .8f, .8f);


                SetEllipsePosition(leftEllipse, scaledJointLeft);
                SetEllipsePosition(rightEllipse, scaledJointRight);


                bool punched;
                var hoverjoint = scaledJointLeft;

                if (handpunch)
                { //right hand punches
                    punched = (skeleton.Joints[JointID.HandRight].Position.Y > skeleton.Joints[JointID.Head].Position.Y);
                     hoverjoint = scaledJointLeft;
                }
                else
                { //left hand punches
                    punched = (skeleton.Joints[JointID.HandLeft].Position.Y > skeleton.Joints[JointID.Head].Position.Y);
                    hoverjoint = scaledJointRight;
                }


                handoverRadio(hoverjoint, punched);
                handoverLetter(hoverjoint, punched);
                handoverStart(hoverjoint, punched);

                //vlad's perfect code
                //....haha
                handoverLevel(hoverjoint, punched);
                handoverProfile(hoverjoint, punched);
                handoverHighScore(hoverjoint, punched);


                //stefan's most perfect code
                handoverExit(hoverjoint, punched);

                lastpunch = punched;

            }
            else
            {
                debugTrack.Text = "Not tracking";
            }
        }


        void startgame()
        {

            if (!gameRunning)
            {
                gameRunning = true;

                mw = new MainWindow();
                //Assign value to children
                mw.nui = this.nui;
                mw.DifficultyLevel = getDiffLevel();


                this.WindowState = WindowState.Normal;

                mw.Show();
                mw.startFromMenu();

                Canvas.SetLeft(leftEllipse,21);
                Canvas.SetTop(leftEllipse, 374);

                mw.Closing += (sender3, e3) => { points = mw.score_points; hss.add_score(DisplayName[0] + DisplayName[1] + DisplayName[2], points); gameRunning = false; };
                mw.Closed += (sender2, e2) => { this.WindowState = WindowState.Normal; mw = null; }; 


            }

        }

        private int getDiffLevel()
        {
            for (int i = 0; i < 4; i++)
                if (radiolist[i].IsChecked == true)
                    return i;

            return 0;
        }

        private void handoverLevel(Joint hoverJoint, bool punched)
        {
            double pos_left = Canvas.GetLeft(levelBtn)+20;
            double pos_top = Canvas.GetTop(levelBtn)+20;

            if (ae(pos_left, hoverJoint.Position.X, 100) &&
                ae(pos_top, hoverJoint.Position.Y, 30))
            {

                levelBtn.Foreground = Brushes.Red;

                if (punched)
                {
                    menu_sound.Play();
                    levelCanvas.Opacity = 1;
                    createprofile.Opacity = 0;
                    highscoreCanvas.Opacity = 0;
                    handpunch ^= true;
                }

            }
            else
            {
                levelBtn.Foreground = Brushes.Black;
            }
        }


        void displayHighScores()
        {
            //show positions
            HSPos.Text = "Pos:\n\n1\n2\n3\n4\n5\n6\n7\n8\n9\n10";

            //show names
            HSName.Text = hss.view_names();

            //show names
            HSPoi.Text = hss.view_points();
        }


        private void handoverHighScore(Joint hoverJoint, bool punched)
        {
            double pos_left = Canvas.GetLeft(highscoreBtn) + 20;
            double pos_top = Canvas.GetTop(highscoreBtn);

            if (ae(pos_left, hoverJoint.Position.X, 100) &&
                ae(pos_top, hoverJoint.Position.Y, 30))
            {

                highscoreBtn.Foreground = Brushes.Red;

                if (punched)
                {

                    displayHighScores();

                    menu_sound.Play();
                    levelCanvas.Opacity = 0;
                    createprofile.Opacity = 0;
                    highscoreCanvas.Opacity = 1;
                    //handpunch ^= true;
                }

            }
            else
            {
                highscoreBtn.Foreground = Brushes.Black;
            }
        }



        private void handoverProfile(Joint hoverJoint, bool punched)
        {
            double pos_left = Canvas.GetLeft(profileBtn)+20;
            double pos_top = Canvas.GetTop(profileBtn);

            if (ae(pos_left, hoverJoint.Position.X, 70) &&
                ae(pos_top, hoverJoint.Position.Y, 30))
            {

                profileBtn.Foreground = Brushes.Red;

                if (punched)
                {
                    menu_sound.Play();

                    createprofile.Opacity = 1;
                    levelCanvas.Opacity = 0;
                    highscoreCanvas.Opacity = 0;
                    handpunch ^= true;

                    //reset name
                    DisplayName[0] = "_";
                    DisplayName[1] = "_";
                    DisplayName[2] = "_";
                    DisplayNameIndex = 0;

                    debugName.Text = "_ _ _";

                }

            }
            else
            {
                profileBtn.Foreground = Brushes.Black;
            }
        }


        private void handoverExit(Joint hoverJoint, bool punched)
        {
            double pos_left = Canvas.GetLeft(exitBtn) + 20;
            double pos_top = Canvas.GetTop(exitBtn);

            if (ae(pos_left, hoverJoint.Position.X, 70) &&
                ae(pos_top, hoverJoint.Position.Y, 30))
            {

                exitBtn.Foreground = Brushes.Red;

                if (punched)
                {
                    menu_sound.Play();
                    //close window
                    this.Close();
                }

            }
            else
            {
                exitBtn.Foreground = Brushes.Black;
            }
        }





        private void handoverStart(Joint hoverJoint, bool punched)
        {
            double pos_left = Canvas.GetLeft(startBtn)+20;
            double pos_top = Canvas.GetTop(startBtn);

            if (ae(pos_left, hoverJoint.Position.X, 70) &&
                ae(pos_top, hoverJoint.Position.Y, 30))
            {

                startBtn.Foreground = Brushes.Red;

                if (punched)
                {
                    menu_sound.Play();
                    startgame();
                    handpunch ^= true;
                }

            }
            else
            {
                startBtn.Foreground = Brushes.Black;
            }
        }


        private void handoverRadio(Joint hoverJoint, bool punched)
        {

            foreach( RadioButton thisRadio in radiolist ) {
                
                //nu imi place hard coding
                double pos_left = Canvas.GetLeft(thisRadio) + 480;
                double pos_top = Canvas.GetTop(thisRadio) + 80;
                
                if (ae(pos_left, hoverJoint.Position.X,50) &&
                    ae(pos_top, hoverJoint.Position.Y,30))
                {
                    thisRadio.Foreground = Brushes.Red;

                    if (punched)
                    {
                        menu_sound.Play();

                        thisRadio.IsChecked = true;
                        thisRadio.Foreground = Brushes.Blue;
                        handpunch ^= true;
                    }

                }
                else
                {
                    thisRadio.Foreground = (thisRadio.IsChecked == true) ? Brushes.Blue : Brushes.Black;
                }

            }

        }


        private void handoverLetter(Joint hoverJoint, bool punched)
        {

            foreach (TextBlock thisText in letterslist)
            {

                double pos_left = Canvas.GetLeft(thisText)+410;
                double pos_top = Canvas.GetTop(thisText)+80;

                if (ae(pos_left, hoverJoint.Position.X, 15) &&
                    ae(pos_top, hoverJoint.Position.Y, 15))
                {
                    thisText.Foreground = Brushes.Red;

                    if (punched && ! lastpunch && DisplayNameIndex < 3)
                    {
                        menu_sound.Play();

                        thisText.Foreground = Brushes.Blue;

                        DisplayName[DisplayNameIndex] = thisText.Text;

                        DisplayNameIndex++;


                        debugName.Text = DisplayName[0] + " " + DisplayName[1] + " " + DisplayName[2];

                        
                        if (DisplayNameIndex == 3)
                            {

                        profileBtn.Text = "Profile: " + DisplayName[0] + DisplayName[1] + DisplayName[2];

                        handpunch ^= true;
                             }

                    }
                     

                }
                else
                {
                    thisText.Foreground = Brushes.Black;
                }

            }

        }



        private bool ae(double bug_left_pos, float p, int tolerance = 50)
        {
            return Math.Abs(bug_left_pos - p) < tolerance;
        }

        private void SetEllipsePosition(FrameworkElement ellipse, Joint hoverJoint)
        {
            Canvas.SetLeft(ellipse, hoverJoint.Position.X);
            Canvas.SetTop(ellipse, hoverJoint.Position.Y);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            nui.Uninitialize();
        }




    }
}
