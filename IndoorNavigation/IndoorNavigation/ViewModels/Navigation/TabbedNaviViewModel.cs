using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace IndoorNavigation.ViewModels.Navigation
{
    public class TabbedNaviViewModel : MvvmHelpers.BaseViewModel
    {
        public TabbedNaviViewModel()
        {
            //test animation of navigation instructions
            DisplayImgTest();
        }

        public async void DisplayImgTest()
        {
            CurrentStepImage = "Arrow_down";
            CurrentStepLabel = "請向後轉";

            NextStepImage = "Arrow_frontleft";
            await Task.Delay(1500);

            CurrentStepImage = "Arrow_frontleft";
            CurrentStepLabel = "請向左前方轉";

            NextStepImage = "Arrow_frontright";
            await Task.Delay(1500);

            CurrentStepImage = "Arrow_frontright";
            CurrentStepLabel = "請向右前方轉";

            NextStepImage = "Arrow_left";
            await Task.Delay(1500);

            CurrentStepImage = "Arrow_left";
            CurrentStepLabel = "請左轉";

            NextStepImage = "Arrow_rearleft";
            await Task.Delay(1500);

            CurrentStepImage = "Arrow_rearleft";
            CurrentStepLabel = "請向左後方轉";

            NextStepImage = "Arrow_rearright";
            await Task.Delay(1500);

            CurrentStepImage = "Arrow_rearright";
            CurrentStepLabel = "請向右後方轉";

            NextStepImage = "Arrow_right";
            await Task.Delay(1500);

            CurrentStepImage = "Arrow_right";
            CurrentStepLabel = "請右轉";

            NextStepImage = "Arrow_up";
            await Task.Delay(1500);

            //CurrentStepImage = "Arrow_up";
            //CurrentStepLabel = "請直走";
            //await Task.Delay(1500);

        }

        private string currentStepLabelName;
        public string CurrentStepLabel
        {
            get
            {
                return currentStepLabelName;
            }

            set
            {
                SetProperty(ref currentStepLabelName, value);
            }
        }

        private string currentStepImageName;
        public string CurrentStepImage
        {
            get
            {
                return string.Format("{0}.png", currentStepImageName);
            }

            set
            {
                if (currentStepImageName != value)
                {
                    currentStepImageName = value;
                    OnPropertyChanged("CurrentStepImage");
                }
            }
        }

        private string nextStepLabelName;
        public string NextStepLabel
        {
            get
            {
                return nextStepLabelName;
            }

            set
            {
                SetProperty(ref nextStepLabelName, value);
            }
        }

        private string nextStepImageName;
        public string NextStepImage
        {
            get
            {
                return string.Format("{0}.png", nextStepImageName);
            }

            set
            {
                if (nextStepImageName != value)
                {
                    nextStepImageName = value;
                    OnPropertyChanged("NextStepImage");
                }
            }
        }
    }
}
