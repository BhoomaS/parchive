using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH.Business
{
  public static class FormattingExtensions
  {
    public static string FormatClockTime(this TimeSpan timeSpan)
    {
      var minDate = DateTime.MinValue;
      return timeSpan.Minutes == 0
        ? minDate.Add(timeSpan).ToString("h tt")
        : minDate.Add(timeSpan).ToString("h:mm tt");
    }

    public static string FormatInterval(this TimeSpan timeSpan)
    {
      var builder = new StringBuilder();
      bool addedTime = false;
      if (timeSpan.Hours > 0)
      {
        builder.Append($"{timeSpan.Hours} hours");
        addedTime = true;
      }

      bool addedMinues = false;
      if (timeSpan.Minutes > 0)
      {
        if (addedTime) builder.Append(", ");
        builder.Append($"{timeSpan.Minutes} minutes");
        addedTime = true;
      }

      if (timeSpan.Seconds > 0)
      {
        if (addedTime) builder.Append(", ");
        builder.Append($"{timeSpan.Seconds} seconds");
      }

      return builder.ToString();
    }

  }

}
