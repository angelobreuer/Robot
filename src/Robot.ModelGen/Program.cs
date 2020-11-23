using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

using var fileStream = new FileStream(@"D:\instances_train2017.json", FileMode.Open);
var root = await JsonSerializer.DeserializeAsync<CocoRoot>(fileStream);
var categories = root.Categories.ToDictionary(x => x.Id);
var images = root.Images.ToDictionary(x => x.Id);
var zip = ZipFile.OpenRead(@"D:\Downloads\train2017.zip");
var dir = new DirectoryInfo(@"D:\out\");

bool FilterCategory(CocoAnnotation annotation)
{
    var category = categories[annotation.CategoryId];
    return category.Name.Equals("person") || category.Name.Equals("chair");
}

var groups = root.Annotations
    .Where(FilterCategory)
    .GroupBy(x => images[x.ImageId])
    .ToArray();

var index = 0;

new Thread(() =>
{
    while (true)
    {
        Console.Write($"\r{index} / {groups.Length} ({Math.Floor((double)root.Annotations.Count / index * 100D)}%)");
        Thread.Sleep(1000);
    }
}).Start();

foreach (var group in groups)
{
    index++;

    var entry = zip.GetEntry($"train2017/{group.Key.Filename}");
    using var stream = entry.Open();
    using var bitmap = Bitmap.FromStream(stream);
    using var resized = ResizeBitmap(bitmap, 416, 416);

    try
    {
        using var file = new FileInfo(Path.Combine(dir.FullName, group.Key.Filename)).OpenWrite();
        resized.Save(file, ImageFormat.Jpeg);
    }
    catch
    {
    }

    var lines = group.Select(x =>
    {
        var origRect = new Rectangle(
            x: (int)x.Bounds[0], y: (int)x.Bounds[1],
            width: (int)x.Bounds[2], height: (int)x.Bounds[3]);

        var rect = Rescale(origRect, bitmap, resized);

        var category = categories[x.CategoryId].Name;
        return $"{category} {rect.X} {rect.Y} {rect.Width} {rect.Height}";
    });

    await File.WriteAllLinesAsync(@"D:\out\" + Path.ChangeExtension(group.Key.Filename, ".txt"), lines);
}

static Rectangle Rescale(Rectangle rectangle, Image oldImage, Image image)
{
    var NewTop = ((rectangle.Top) * image.Height / oldImage.Height);
    var NewLeft = ((rectangle.Left) * image.Width / oldImage.Width);

    var NewBottom = ((rectangle.Bottom + 1) * image.Height / oldImage.Height) - 1;
    var NewRight = ((rectangle.Right + 1) * image.Width / oldImage.Width) - 1;

    return new Rectangle(NewLeft, NewTop, NewRight - NewLeft, NewBottom - NewTop);
}

Bitmap ResizeBitmap(Image bmp, int width, int height)
{
    var result = new Bitmap(width, height);

    using (var graphics = Graphics.FromImage(result))
    {
        graphics.DrawImage(bmp, 0, 0, width, height);
    }

    return result;
}

internal class CocoAnnotation
{
    [JsonPropertyName("bbox")]
    public float[] Bounds { get; set; }

    [JsonPropertyName("category_id")]
    public long CategoryId { get; set; }

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("image_id")]
    public long ImageId { get; set; }
}

internal class CocoCategory
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}

internal class CocoImage
{
    [JsonPropertyName("file_name")]
    public string Filename { get; set; }

    [JsonPropertyName("id")]
    public long Id { get; set; }
}

internal class CocoRoot
{
    [JsonPropertyName("annotations")]
    public IList<CocoAnnotation> Annotations { get; set; }

    [JsonPropertyName("categories")]
    public IList<CocoCategory> Categories { get; set; }

    [JsonPropertyName("images")]
    public IList<CocoImage> Images { get; set; }
}
