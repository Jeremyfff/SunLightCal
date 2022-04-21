using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class Time {
    int month;
    int day;
    int hour;
    int minute;

    public Time(int month, int day, int hour, int minute) {
        this.month = month;
        this.day = day;
        this.minute = minute;
        this.hour = hour;
    }




    static public int GetDayIndex(int month, int day) {

        int index = 0;
        for (int i = 0; i < month - 1; i++) {
            index += Time.GetMonthDayCount(i);
        }
        index += day - 1;
        return index;
    }

    public int ConvertToDayIndex() {
        return Time.GetDayIndex(this.month, this.day);
    }
    static public int GetMonthDayCount(int month) {
        switch (month) {
            case 1: return 31;
            case 2: return 29;
            case 3: return 31;
            case 4: return 30;
            case 5: return 31;
            case 6: return 30;
            case 7: return 31;
            case 8: return 31;
            case 9: return 30;
            case 10: return 31;
            case 11: return 30;
            case 12: return 31;
            default: return 0;
        }
    }

    public int GetMonthDayCount() {
        return Time.GetMonthDayCount(this.month);
    }

    static public int ConvertToMinuteIndex(int hour, int minute) {
        int index = 60 * hour + minute;
        if (index >= 0 && index <= 60 * 24) {
            return index;
        } else {
            return -1;
        }
    }

    public int ConvertToMinuteIndex() {
        return Time.ConvertToMinuteIndex(this.hour, this.minute);
    }

    static public Time ConvertFromMinuteIndex(int index) {
        var hour = (int)(index / 60);
        var minute = index - hour * 60;

        return new Time(0, 0, hour, minute);
    }
    public float ConvertToHours() {
        return this.ConvertToMinuteIndex() / 60f;
    }

    public Time Set(int month, int day, int hour, int minute) {
        if (month != -1) {
            this.month = month;
        }
        if (day != -1) {
            this.day = day;
        }
        if (minute != -1) {
            this.minute = minute;
        }
        if (hour != -1) {
            this.hour = hour;
        }

        return this;
    }

    public Time Copy() {
        return new Time(this.month, this.day, this.hour, this.minute);
    }

    static public Time TimeAdd(Time time1, Time time2) {

        var _m = time1.minute + time2.minute;
        var _h = time1.hour + time2.hour;

        _h += _m >= 0 ? (int)(_m / 60) : (int)(_m / 60) - 1;

        var m = _m % 60;

        var h = _h % 24;


        return new Time(time1.month + time2.month, time1.day + time2.day, h, m);
    }
    public Time Add(Time time2) {
        return Time.TimeAdd(this, time2);
    }
    static public Time Multiply(Time time, float a) {
        var minutes = time.ConvertToMinuteIndex();
        minutes = (int)(minutes * a);
        return Time.ConvertFromMinuteIndex(minutes);

    }
    public Time Multiply(float a) {
        return Time.Multiply(this, a);
    }

    static public Time Reverse(Time time) {
        return new Time(-time.month, -time.day, -time.hour, -time.minute);
    }

    public Time Reverse() {
        return Time.Reverse(this);
    }

    override public string ToString() {
        return "Time:[month:" + this.month + ",day:" + this.day + ",hour:" + this.hour + ",minute:" + this.minute + "]";
    }

}

