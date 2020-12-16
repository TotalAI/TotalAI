using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    public class TimeManager : MonoBehaviour
    {
        public int startDayOfWeek = 1;
        public int startGameHour = 12;
        public float realTimeInMinutesPerDay = 2;

        public int sunriseHour = 6;
        public int sunsetHour = 20;

        public float dayLightLevel = 1f;
        public float nightLightLevel = .1f;

        //private Light sun;

        public float RealTimeSecondsPerGameMinute()
        {
            return realTimeInMinutesPerDay / 24f;
        }

        public bool IsNight()
        {
            return TwentyFourHour() >= sunsetHour || TwentyFourHour() < sunriseHour;
        }

        public float CurrentDayLight()
        {
            if (IsNight())
                return nightLightLevel;
            return dayLightLevel;
        }

        // One hour is = realTimeInMinutesPerDay / 24 ( one hour  = 2 / 24 = 1/12 of a minute * 60 = 5 seconds
        public int Minutes()
        {
            return (int)((Time.time % (realTimeInMinutesPerDay / 24 * 60) / (realTimeInMinutesPerDay / 24 * 60)) * 60);
        }

        public int TwentyFourHour()
        {
            // Every X minutes a complete day goes by - so when Time.time % (realTimeInMinutesPerDay * 60) == 0 it is startGameHour
            float percentAfterStartGameHour = (Time.time % (realTimeInMinutesPerDay * 60)) / (realTimeInMinutesPerDay * 60);
            return ((int)(percentAfterStartGameHour * 24) + startGameHour) % 24;
        }

        // Returns the number of minutes in this day - since the day started
        public int MinutesIntoDay()
        {
            return TwentyFourHour() * 60 + Minutes();
        }

        public string PrettyMinutes()
        {
            int minutes = Minutes();
            return minutes < 10 ? "0" + minutes : minutes.ToString();
        }

        // First day is Day 1 - realTimeInMinutesPerDay - startGameHour
        public int DaysSinceStart()
        {
            return (int)((Time.time + startGameHour / 24.0f * realTimeInMinutesPerDay * 60) / (realTimeInMinutesPerDay * 60)) + 1;
        }

        public int Day()
        {
            return DaysSinceStart() % 120;
        }

        public int Year()
        {
            return DaysSinceStart() / 120;
        }

        public int DayOfWeek()
        {
            return (DaysSinceStart() - 1 + startDayOfWeek - 1) % 7 + 1;
        }

        public string PrettyPrintDateTime()
        {
            return "Day " + Day() + ", " + Year() + " - " + TwentyFourHour() + ":" + PrettyMinutes();
        }

        void Start()
        {
            //sun = RenderSettings.sun;
            //StartCoroutine(MainTimeLoop());
        }

        private IEnumerator MainTimeLoop()
        {
            while (true)
            {
                if (IsNight())
                {
                    //sun.intensity = nightLightLevel;
                    RenderSettings.ambientIntensity = nightLightLevel;
                }
                else
                {
                    //sun.intensity = dayLightLevel;
                    RenderSettings.ambientIntensity = dayLightLevel;
                }

                yield return new WaitForSeconds(1f);
            }
        }
    }
}