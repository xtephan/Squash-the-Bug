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
using System.Windows.Forms;


//Kinect Libraries
using Microsoft.Research.Kinect.Nui;
using Coding4Fun.Kinect.Wpf; 

//media
using System.Media;
using System.Windows.Media.Animation;

using System.Threading;
using System.Windows.Threading;


namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //Vlad's global variables
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Timer timer2;
        private int counter = 60;
        private int counter2;
        private int move_mon = 0;
        private bool playing = false;


        //Stefan's global vars
        //Runtime nui = Runtime.Kinects[0];
        Runtime nui;
        private short MaxX= 640;
        private short MaxY= 860;

        private int score_points = 0;

        SoundPlayer squash_sound, backround_sound;



        //random X,Y -- move bug
        private static Random r = new Random();
        
        private void bug_ellipse()
        {
            int X = r.Next(610);
            int Y = r.Next(450);
            Canvas.SetLeft(bug, X);
            Canvas.SetTop(bug, Y);
        }

        private void pausebutton(object sender, EventArgs e)
        {

            playing = false;

            timer1.Stop();
            timeBlock.Text = counter.ToString();

            timer2.Stop();
            counter2 = 60;

        }

        private void stopbutton(object sender, EventArgs e)
        {
            playing = false;

            timer1.Stop();
            counter = 60;
            if (counter == 60)
                timer2.Stop();
            counter2 = 60;
            timeBlock.Text = counter.ToString();

        }

        //countdown
        private void startbutton(object sender, EventArgs e)
        {
            playing = true;

            //int counter = 60;
            timer1 = new System.Windows.Forms.Timer();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = 1000;
            timer1.Start();
            timeBlock.Text = counter.ToString();

            
            int counter2 = 60;
            timer2 = new System.Windows.Forms.Timer();
            timer2.Tick += new EventHandler(timer2_Tick);
            //timer2.Interval = 1000;
            timer2.Start();
                                    
            string Level = "Level selected: EASY";
            if (easybttn.IsChecked == true)
            {                
                textBlock1.Text = Level;
                timer2.Interval = 3000;
            }

            string Level1 = "Level selected: MEDIUM";
            if (medbttn.IsChecked == true)
            {
                textBlock1.Text = Level1;
                timer2.Interval = 2000;
            }

            string Level2 = "Level selected: HARD";
            if (hardbttn.IsChecked == true)
            {
                textBlock1.Text = Level2;
                timer2.Interval = 1000;
            }

            string Level3 = "Level selected: INSANE";
            if (insbttn.IsChecked == true)
            {
                textBlock1.Text = Level3;
                timer2.Interval = 500;
            }


        }

        private int k = 0;

        private void muta_gandac()
        {
            if (k < 60)
                bug_ellipse();
        }
        
        private void timer2_Tick(object sender, EventArgs e)
        {
            k = k + timer2.Interval / 1000;
            muta_gandac();
        }
       
        private void timer1_Tick(object sender, EventArgs e)
        {
            counter--;
            if (counter == 0)
            {
                playing = false;
                timer1.Stop();
            }
            timeBlock.Text = counter.ToString();

        }



        public MainWindow()
        {
            InitializeComponent();
        }

        public double Y { get; set; }

        public double X { get; set; }


        /* 
         * ***********************************************************
         */
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            squash_sound = new SoundPlayer("squash_sound.wav");
            backround_sound = new SoundPlayer("fly_sound_wind.wav");

            backround_sound.PlayLooping();

            SetupKinect();


        }


        void SetupKinect()
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
                nui.Initialize(RuntimeOptions.UseSkeletalTracking);

                //add event to receive skeleton data
                nui.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);


                nui.SkeletonEngine.TransformSmooth = true;
                TransformSmoothParameters parameters = new TransformSmoothParameters();
                parameters.Smoothing = 0.7f;
                parameters.Correction = 0.9f;
                parameters.Prediction = 0.9f;
                parameters.JitterRadius = 1.0f;
                parameters.MaxDeviationRadius = 0.5f;
                nui.SkeletonEngine.SmoothParameters = parameters;


            }

        }


        void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {

            SkeletonFrame allSkeletons = e.SkeletonFrame;

            //get the first tracked skeleton
            SkeletonData skeleton = (from s in allSkeletons.Skeletons
                                     where s.TrackingState == SkeletonTrackingState.Tracked
                                     select s).FirstOrDefault();


            if (skeleton != null)
            {
                TrackBug.Text = "Tracking";

                //set position
                //SetEllipsePosition(headEllipse, skeleton.Joints[JointID.Head]);
                SetEllipsePosition(leftEllipse, skeleton.Joints[JointID.HandLeft]);
                SetEllipsePosition(rightEllipse, skeleton.Joints[JointID.HandRight]);

                if (bug_squashed(skeleton.Joints[JointID.HandLeft], skeleton.Joints[JointID.HandRight]))
                {

                    double bloodX = Canvas.GetLeft(bug);
                    double bloodY = Canvas.GetTop(bug);

                    muta_gandac();
                                        
                    squash_sound.Play();

                    score_points++;
                    score_display.Text="Score: " + score_points.ToString();

                    Canvas.SetLeft(sange,bloodX);
                    Canvas.SetTop(sange, bloodY);
                     
                    DoubleAnimation myDoubleAnimation = new DoubleAnimation();
                    myDoubleAnimation.From = 1.0;
                    myDoubleAnimation.To = 0.0;

                    myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(1.5));

                    Storyboard myStoryboard;

                    myStoryboard = new Storyboard();
                    myStoryboard.Children.Add(myDoubleAnimation);

                    Storyboard.SetTargetName(myDoubleAnimation, "sange");

                    //change with ellipse or imagebrush
                    Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(Image.OpacityProperty));

                    myStoryboard.Begin(this);
                    
                    
                }


            }
            else
            {
                TrackBug.Text = "NOT tracking";
            }
        }



        private bool bug_squashed(Joint joint, Joint joint_2)
        {
            if (!playing)
                return false;

            var left = joint.ScaleTo(640, 480, .8f, .8f);
            var right = joint_2.ScaleTo(640, 480, .8f, .8f);

            
            double bug_left_pos = Canvas.GetLeft(bug);
            double bug_top_pos = Canvas.GetTop(bug);


            if ( ae(bug_left_pos, left.Position.X) &&
                 ae(bug_top_pos, left.Position.Y) )
                return true;


            if (ae(bug_left_pos, right.Position.X) &&
                ae(bug_top_pos,  right.Position.Y))
                return true;

            return false;
        }

        private bool ae(double bug_left_pos, float p)
        {
            return Math.Abs(bug_left_pos - p) < 25;
        }


        private void SetEllipsePosition(FrameworkElement ellipse, Joint joint)
        {
            var scaledJoint = joint.ScaleTo(640, 480, .8f, .8f);
            //var scaledJoint = joint.ScaleTo(350, 350, .8f, .8f);

            Canvas.SetLeft(ellipse, scaledJoint.Position.X);
            Canvas.SetTop(ellipse, scaledJoint.Position.Y);

        }


        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            //nui.Uninitialize();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            nui.Uninitialize();
        }


    }
}
