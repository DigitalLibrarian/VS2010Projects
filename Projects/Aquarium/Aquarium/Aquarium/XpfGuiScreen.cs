using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.Screens;
using RedBadger.Xpf.Adapters.Xna.Graphics;
using RedBadger.Xpf.Controls;
using RedBadger.Xpf.Media;
using RedBadger.Xpf;
using RedBadger.Xpf.Adapters.Xna.Input;
using RedBadger.Xpf.Data;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;
using RedBadger.Xpf.Media.Imaging;
using System.Reactive;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace Aquarium
{

     public class ApplicationBarIconButton
    {
        private readonly ImageSource iconImageSource;

        private readonly string text;

        public ApplicationBarIconButton(string text, ImageSource iconImageSource)
        {
            this.text = text;
            this.iconImageSource = iconImageSource;
        }

        public ImageSource IconImageSource
        {
            get
            {
                return this.iconImageSource;
            }
        }

        public string Text
        {
            get
            {
                return this.text;
            }
        }
    }

     public class ApplicationBar : ContentControl
     {
         private readonly IList<ApplicationBarIconButton> buttons = new List<ApplicationBarIconButton>();

         private readonly ISubject<ApplicationBarIconButton> clicks = new Subject<ApplicationBarIconButton>();

         public IList<ApplicationBarIconButton> Buttons
         {
             get
             {
                 return this.buttons;
             }
         }

         public IObservable<ApplicationBarIconButton> Clicks
         {
             get
             {
                 return this.clicks;
             }
         }

         public override void OnApplyTemplate()
         {
             if (this.buttons.Count > 4)
             {
                 throw new NotSupportedException("Too many buttons - the maximum is 4.");
             }

             this.Height = 70;

             var containingBorder = new Border { Background = new SolidColorBrush(new Color(31, 31, 31, 255)) };

             var itemsControl = new ItemsControl
             {
                 ItemsPanel = new StackPanel { Orientation = Orientation.Horizontal },
                 ItemsSource = this.buttons,
                 ItemTemplate = dataContext =>
                 {
                     var image = new Image { Stretch = Stretch.None };
                     image.Bind(
                         Image.SourceProperty,
                         BindingFactory.CreateOneWay<ApplicationBarIconButton, ImageSource>(
                             iconButton => iconButton.IconImageSource));

                     var button = new Button { Content = image, Margin = new Thickness(18, 0, 18, 0) };

                     //Observable.FromEvent<EventArgs>(
                     //    handler => button.Click += handler,
                     //    handler => button.Click -= handler).Select(
                     //        eventArgs => (ApplicationBarIconButton)((Button)eventArgs.Sender).DataContext).
                     //    Subscribe(this.clicks);

                     return button;
                 },
                 HorizontalAlignment = HorizontalAlignment.Center
             };

             containingBorder.Child = itemsControl;

             this.Content = containingBorder;
         }
     }

    public class XpfGuiScreen : DevScreen
    {

        private readonly IList<ApplicationBarIconButton> buttons = new List<ApplicationBarIconButton>();
        private readonly ISubject<ApplicationBarIconButton> clicks = new Subject<ApplicationBarIconButton>();

        SpriteBatchAdapter spriteBatchAdapter { get; set; }
        SpriteFontAdapter SpriteFontAdapter { get; set; }
        RootElement rootElement { get; set; }

        public  void LoadContentPoop()
        {
            var gd = ScreenManager.Game.GraphicsDevice;
            this.spriteBatchAdapter = new SpriteBatchAdapter(new SpriteBatch(gd));
            var primitivesService = new PrimitivesService(gd);
            var renderer = new Renderer(this.spriteBatchAdapter, primitivesService);

            var spriteFontAdapter = new SpriteFontAdapter(ScreenManager.Game.Content.Load<SpriteFont>("Segoe18"));
            var largeFont = new SpriteFontAdapter(ScreenManager.Game.Content.Load<SpriteFont>("Segoe30"));

            var addButtonImageTexture =
                new TextureImage(new Texture2DAdapter(ScreenManager.Game.Content.Load<Texture2D>("AddButton")));
            var trashButtonImageTexture =
                new TextureImage(new Texture2DAdapter(ScreenManager.Game.Content.Load<Texture2D>("TrashButton")));

            this.rootElement = new RootElement(gd.Viewport.ToRect(), renderer, new InputManager());

            var buttonClickResults = new ObservableCollection<string>();

            var header1 = new TextBlock(spriteFontAdapter)
            {
                Text = "MY APPLICATION",
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(10)
            };
            var header2 = new TextBlock(largeFont)
            {
                Text = "XNA Application Bar",
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(10)
            };
            var itemsControl = new ItemsControl
            {
                ItemsSource = buttonClickResults,
                ItemTemplate = _ =>
                {
                    var textBlock = new TextBlock(spriteFontAdapter)
                    {
                        Foreground = new SolidColorBrush(Colors.White)
                    };
                    textBlock.Bind(
                        TextBlock.TextProperty, BindingFactory.CreateOneWay<string>());
                    return textBlock;
                }
            };

            var scrollViewer = new ScrollViewer { Content = itemsControl };

            var applicationBar = new ApplicationBar
            {
                Buttons =
                        {
                            new ApplicationBarIconButton("Add", addButtonImageTexture), 
                            new ApplicationBarIconButton("Delete", trashButtonImageTexture)
                        }
            };

            var grid = new Grid
            {
                Background = new SolidColorBrush(Colors.Black),
                RowDefinitions =
                        {
                            new RowDefinition { Height = GridLength.Auto }, 
                            new RowDefinition { Height = GridLength.Auto }, 
                            new RowDefinition(), 
                            new RowDefinition { Height = new GridLength(70) }
                        },
                Children =
                        {
                            header1, 
                            header2, 
                            scrollViewer,
                            applicationBar
                        }
            };

            applicationBar.Clicks.Subscribe(
                Observer.Create<ApplicationBarIconButton>(s => buttonClickResults.Add(s.Text)));

            Grid.SetRow(header1, 0);
            Grid.SetRow(header2, 1);
            Grid.SetRow(scrollViewer, 2);
            Grid.SetRow(applicationBar, 3);

            this.rootElement.Content = grid;
        }

        public override void LoadContent()
        {
            base.LoadContent();

            var gd = ScreenManager.Game.GraphicsDevice;
            spriteBatchAdapter = new SpriteBatchAdapter(ScreenManager.SpriteBatch); 
            var primitivesService = new PrimitivesService(gd);
            var renderer = new Renderer(spriteBatchAdapter, primitivesService);

            rootElement = new RootElement(gd.Viewport.ToRect(), renderer, new InputManager());
            SpriteFontAdapter = new SpriteFontAdapter(ScreenManager.Font);

            rootElement.Content = TestTextBlock();
        }

        IElement ActionBar()
        {

            var button = new Button()
            {
                Margin = new Thickness(10),
                Content = new TextBlock(SpriteFontAdapter) { Text = "BUTT", Margin = new Thickness(100) }
            };
            button.IsEnabled = true;
            button.Click += new EventHandler<EventArgs>(button_Click);
            return new StackPanel
            {
                Background = new SolidColorBrush(Colors.Red),
                Children =
                    {
                        button,
                        new TextBlock(SpriteFontAdapter) { Text = "Item 2", Margin = new Thickness(10) },
                        new TextBlock(SpriteFontAdapter) { Text = "Item 3", Margin = new Thickness(10) }
                    },

                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom
            };
        }

        IElement TestTextBlock()
        {
            return new TextBlock(SpriteFontAdapter) { 
                Background = new SolidColorBrush(Colors.White),
                Text = "THis is a long string\r\n that spans \r\nmany\r\nlines\r\n", Margin= new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        IElement ItemsControl()
        {
           return new ItemsControl
                {
                    ItemsPanel = new StackPanel { Orientation = Orientation.Horizontal }, 
                    ItemsSource = this.buttons,
                    ItemTemplate = (dataContext) =>
                        {
                            var image = new Image { Stretch = Stretch.None };
                            image.Bind(
                                Image.SourceProperty, 
                                BindingFactory.CreateOneWay<ApplicationBarIconButton, ImageSource>(
                                    iconButton => iconButton.IconImageSource));

                            var button = new Button { Content = image, Margin = new Thickness(18, 0, 18, 0) };
                            return button;
                        },
                        
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                    
        }

        IElement Canvas()
        {
            return new Canvas();
        }

        IElement Border()
        {
            var grid = new Grid
            {
                Background = new SolidColorBrush(Colors.White),

                RowDefinitions =
                        {
                            new RowDefinition(),
                            new RowDefinition { Height = GridLength.Auto },
                            new RowDefinition()
                        },
                                ColumnDefinitions =
                        {
                            new ColumnDefinition { Width = GridLength.Auto },
                            new ColumnDefinition { Width = GridLength.Auto }
                        }
            };
            var topLeftBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(0, 0, 0, 2),
                Child = new TextBlock(SpriteFontAdapter)
                {
                    Text = "Top Left",
                    Margin = new Thickness(10)
                }
            };
            Grid.SetRow(topLeftBorder, 0);
            Grid.SetColumn(topLeftBorder, 0);
            grid.Children.Add(topLeftBorder);

            var topRightBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(0, 0, 0, 2),
                Child = new TextBlock(SpriteFontAdapter)
                {
                    Text = "Top Right",
                    Margin = new Thickness(10),
                    HorizontalAlignment = HorizontalAlignment.Right
                }
            };
            Grid.SetRow(topRightBorder, 0);
            Grid.SetColumn(topRightBorder, 1);
            grid.Children.Add(topRightBorder);

            var bottomLeftBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(0, 2, 0, 0),
                Background = new SolidColorBrush(new Color(106, 168, 79, 255)),
                Child = new TextBlock(SpriteFontAdapter)
                {
                    Text = "Bottom Left",
                    Margin = new Thickness(10),
                    VerticalAlignment = VerticalAlignment.Bottom
                }
            };
            Grid.SetRow(bottomLeftBorder, 1);
            Grid.SetColumn(bottomLeftBorder, 0);
            grid.Children.Add(bottomLeftBorder);

            var bottomRightBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(0, 2, 0, 0),
                Background = new SolidColorBrush(new Color(106, 168, 79, 255)),
                Child = new TextBlock(SpriteFontAdapter)
                {
                    Text = "Bottom Right",
                    Margin = new Thickness(10),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom
                }
            };
            Grid.SetRow(bottomRightBorder, 1);
            Grid.SetColumn(bottomRightBorder, 1);
            grid.Children.Add(bottomRightBorder);

            return grid;
        }

        void button_Click(object sender, EventArgs e)
        {

        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            rootElement.Draw();

            base.Draw(gameTime);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            rootElement.Update();

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }
    }
}
