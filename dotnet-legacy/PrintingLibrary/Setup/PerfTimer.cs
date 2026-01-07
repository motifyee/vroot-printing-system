
namespace PrintingLibrary.Setup;

public class PerfTimer(string printMsg) {
  DateTime _dateOne = DateTime.Now;
  string _message = printMsg;

  public void Print(string? resetMsg = null) {
    var dateTwo = DateTime.Now;
    var diff = dateTwo.Subtract(_dateOne);
    Console.WriteLine(String.Format("{0} {1}:{2}", _message, diff.Seconds, diff.Milliseconds));

    if (resetMsg != null)
      Reset(resetMsg);
  }

  public void End() {
    Print("");
  }

  public void Reset(string printMsg) {
    _message = printMsg;
    _dateOne = DateTime.Now;
  }
}