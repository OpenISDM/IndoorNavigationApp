using System;
using System.Collections.Generic;
using System.Linq;
using IndoorNavigation.ViewModels;
using Xamarin.Forms;

namespace IndoorNavigation.Views.Navigation
{
    public partial class TabbedPageRoutes : ContentPage
    {
        public TabbedPageRoutes()
        {
            InitializeComponent();

            //RoutesListView.ItemsSource = GetRouteList();
        }

        void RoutesListView_ItemSelected(object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {
            // disable it
            RoutesListView.SelectedItem = null;
        }

        //TODO: add its method after get algorithm computed data
        // test fake data group and put into listview
        //private static IEnumerable<Grouping<string, RoutesDataClass>> GetRouteList()
        //{
        //    List<RoutesDataClass> routes = new List<RoutesDataClass>
        //    {
        //        // current step is set OpacityValue to 1, others set to 0.3
        //        new RoutesDataClass{ Order=1, OpacityValue=0.3, ImageSrc="Arrow_up.png", Instruction="往前方路口直走", Floor="1樓" },
        //        new RoutesDataClass{ Order=2, OpacityValue=1, ImageSrc="Arrow_left.png", Instruction="向左轉", Floor="1樓" },
        //        new RoutesDataClass{ Order=3, OpacityValue=0.3, ImageSrc="Arrow_up.png", Instruction="請直走", Floor="1樓" },
        //        new RoutesDataClass{ Order=4, OpacityValue=0.3, ImageSrc="Arrow_frontleft.png", Instruction="向左前方轉", Floor="2樓" },
        //        new RoutesDataClass{ Order=5, OpacityValue=0.3, ImageSrc="Arrow_up.png", Instruction="請直走", Floor="2樓" },
        //        new RoutesDataClass{ Order=6, OpacityValue=0.3, ImageSrc="Arrow_frontright.png", Instruction="向右前方直走", Floor="2樓" },
        //        new RoutesDataClass{ Order=7, OpacityValue=0.3, ImageSrc="Arrow_right.png", Instruction="向右轉", Floor="2樓" },
        //        new RoutesDataClass{ Order=8, OpacityValue=0.3, ImageSrc="Arrow_up.png", Instruction="請直走", Floor="3樓" }
        //    };

        //    return from route in routes
        //           orderby route.Order
        //           group route by route.Floor into routeGroup
        //           orderby int.Parse(routeGroup.Key.Split('樓')[0])
        //           select new Grouping<string, RoutesDataClass>(routeGroup.Key, routeGroup);
        //}

    }

    class RoutesDataClass
    {
        public int Order { get; set; }
        public double OpacityValue { get; set; }
        public string ImageSrc { get; set; }
        public string Instruction { get; set; }
        public string Floor { get; set; }
    }

}
