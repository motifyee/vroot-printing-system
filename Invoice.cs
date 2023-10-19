
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TodoApi;

public class Invoice
{//: IParsable<Invoice>
    public string? PrinterName { get; set; }
    public string? TemplateName { get; set; }

    public string? Company { get; set; }
    public string? Cashier { get; set; }
    public string? Branch { get; set; }
    public string? BranchDesc { get; set; }


    public string? Date { get; set; }
    public string? Time { get; set; }

    public string? InvoiceNo { get; set; }

    public string? InvoiceType { get; set; }


    public string? ClientName { get; set; }
    public string? ClientPhone1 { get; set; }
    public string? ClientPhone2 { get; set; }
    public string? ClientAddress { get; set; }
    public string? ClientArea { get; set; }


    // uses reflection to retrieve a self-value by name
    private string? GetValue(string propertyName)
    {
        Type type = typeof(Invoice);
        System.Reflection.PropertyInfo? info = type.GetProperty(propertyName);
        return (string?)info?.GetValue(this, null)?.ToString();
    }
    private List<string> SplitLongValueLines(string value, int maxLineLength = 40)
    {
        var line = "";
        var lines = new List<string>();
        foreach (var wd in value.Split(" "))
        {
            if (line.Length < maxLineLength && line.Length + wd.Length <= maxLineLength)
                line += $" {wd}";
            else
            {
                lines.Add(line);
                line = "";
            }
        };
        if (line.Trim().Length > 0)
            lines.Add(line);

        return lines;
    }
    // converts a some properties to an iterable for MiniExcel
    private List<Entry> GetPropertyListEntries(Dictionary<string, string> properties, List<string> multilineProperties)
    {
        var entries = new List<Entry>();

        foreach (KeyValuePair<string, string> prop in properties)
        {
            var value = GetValue(prop.Key);
            if (value == null || value.Trim().Length == 0) continue;

            if (!multilineProperties.Contains(prop.Key))
            {
                entries.Add(new Entry()
                {
                    Title = prop.Value,
                    Value = value,
                    // Values = new List<string?> { value }
                });
                continue;
            }

            var lines = SplitLongValueLines(value);

            // TODO_BESAFE if sent is null
            for (var i = 0; i < lines.Count; i++)
            {
                entries.Add(new Entry()
                {
                    Title = i == 0 ? prop.Value : null,
                    Value = lines[i],
                    // Values = new List<string?> { lines[i] }
                });
            }

        }

        return entries;
    }



    public List<Entry> ClientInfo
    {
        get
        {
            var properties = new Dictionary<string, string>(){
                 {"ClientName","عميل"},
                 {"ClientPhone1","موبايل"},
                 {"ClientPhone2","موبايل آخر"},
                 {"ClientAddress","عنوان"},
                 {"ClientArea", "منطقة"}
            };

            return GetPropertyListEntries(properties, new List<string> { "ClientAddress" });
        }
    }

    public string? DeliveryName { get; set; }
    public string? DeliveryNameTitle
    {
        get
        {
            return (DeliveryName != null && DeliveryName?.Length > 0) ? "طيار" : null;
        }
    }

    public List<Item> Items { get; set; } = new List<Item>();

    public List<Entry> ItemsInfo
    {
        get
        {
            var entries = new List<Entry>();

            foreach (var item in Items)
            {
                entries.Add(new Entry()
                {
                    Title = item?.Title,
                    Value = item?.Count,
                    // Values = new List<string?> { item?.Count }
                });
                if (item?.Note != null && item?.Note?.Trim().Length > 0)
                {
                    Console.WriteLine(item?.Title);
                    entries.Add(new Entry()
                    {
                        Title = $" — {item?.Note} —",
                    });
                }
            }

            return entries;
        }
    }

    public List<Item> EditedItems { get; set; } = new List<Item>();
    public string? EditedTitle
    {
        get
        {
            return (EditedItems != null && EditedItems?.Count > 0) ? "تعديل" : null;
        }
    }
    public List<Entry> EditedItemsInfo
    {
        get
        {
            var entries = new List<Entry>();

            foreach (var item in EditedItems)
            {
                entries.Add(new Entry()
                {
                    Title = item?.Title,
                    Value = item?.Count,
                    // Values = new List<string?> { item?.Count }
                });
                if (item?.Note != null && item?.Note?.Trim().Length > 0)
                {
                    Console.WriteLine(item?.Title);
                    entries.Add(new Entry()
                    {
                        Title = item?.Note,
                    });
                }
            }

            return entries;
        }
    }

    public string? SectionName { get; set; }

    public string? ScheduleTime { get; set; }
    public string? ScheduleTitle
    {
        get
        {
            return (ScheduleTime != null && ScheduleTime?.Length > 0) ? "حجز" : null;
        }
    }

    public string? TableNo { get; set; }
    public string? TableTitle
    {
        get
        {
            return (TableNo != null && TableNo?.Length > 0) ? "طاولة" : null;
        }
    }




    public string? Discount { get; set; }
    public string? Service { get; set; }
    public string? Delivery { get; set; }
    public string? Vat { get; set; }
    public string? Total { get; set; }

    public List<Entry> InvoiceInfo
    {
        get
        {
            var properties = new Dictionary<string, string>(){
                 {"Discount","الخصم"},
                 {"Service","Service 12%"},
                 {"Delivery","خدمة توصيل"},
                 {"Vat","Vat 14%"},
                 {"Total", "اجمالي"}
            };

            return GetPropertyListEntries(properties, new List<string> { "ClientAddress" });
        }
    }
    public string? Note { get; set; }
    public string? PrintingDate { get; set; }
    public string? PrintingTime { get; set; }

    // public List<string?> FooterNotes { get; set; } = new List<string?>();
    public string? FooterNote1 { get; set; }
    public string? FooterNote2 { get; set; }
    public string? FooterNote3 { get; set; }

}



internal class DateTimeConverter : JsonConverter<DateTime>
{

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => JsonSerializer.Deserialize<DateTime>(ref reader, options);

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    => writer.WriteStringValue(value.ToString()); //writer.WriteStringValue(value);
}

public class Entry
{
    // [JsonConverter(typeof(StringConverter))]
    public string? Title { get; set; }
    // public List<string?> Values { get; set; } = new List<string?>();
    public string? Value { get; set; }
    public string? Value2 { get; set; }
    public string? Value3 { get; set; }
}
public class Item
{
    // [JsonConverter(typeof(StringConverter))]
    public string? Title { get; set; }
    public string? Count { get; set; }
    public string? Price { get; set; }
    public string? TotalPrice { get; set; }
    public string? Note { get; set; }
}

