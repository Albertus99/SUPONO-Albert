using TMPro;
using UnityEngine;

namespace Supono.App.Windows
{
    public sealed class DailyRewardDayView : MonoBehaviour
    {
        [SerializeField] TMP_Text dayLabel;
        [SerializeField] TMP_Text amountLabel;
        [SerializeField] GameObject claimedIcon;
        [SerializeField] GameObject todayHighlight;
        [SerializeField] CanvasGroup content;

        public void Bind(int day, int amount, bool claimed, bool isToday)
        {
            dayLabel.text = $"Day {day}";
            amountLabel.text = amount.ToString();
            claimedIcon.SetActive(claimed);
            todayHighlight.SetActive(isToday);
            content.alpha = claimed || isToday ? 1f : 0.55f;
        }

        public void MarkClaimed()
        {
            claimedIcon.SetActive(true);
            todayHighlight.SetActive(false);
        }
    }
}
