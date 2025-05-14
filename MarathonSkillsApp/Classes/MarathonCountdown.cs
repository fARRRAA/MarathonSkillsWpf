using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MarathonSkillsApp.Classes
{
    public class MarathonCountdown
    {
        private Action<string> updateAction;
        private DateTime targetDate;
        private DispatcherTimer timer;

        public MarathonCountdown(Action<string> updateAction, DateTime targetDate)
        {
            this.updateAction = updateAction;
            this.targetDate = targetDate;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        public MarathonCountdown(Action<string> updateCountdownText)
        {
            this.updateAction = updateCountdownText;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeSpan timeLeft = targetDate - DateTime.Now;

            if (timeLeft.TotalSeconds <= 0)
            {
                updateAction("Марафон начался!");
                timer.Stop();
                return;
            }

            string countdownText = $"{timeLeft.Days} дней {timeLeft.Hours} часов и {timeLeft.Minutes} минут до старта марафона!";
            updateAction(countdownText);
        }

        public void Stop()
        {
            timer.Stop();
        }
    }
}
