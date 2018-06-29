using System;
using CoreLocation;
using Foundation;
using MapKit;
using UIKit;
using CoreGraphics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MapkitDirections {

  public class MyViewController : UIViewController {

    //    private static string mDocumentRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    private string document_root = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory,
                                                                        NSSearchPathDomain.User)[0].Path;

    //    private static string mLibraryRoot = Path.GetFullPath(Path.Combine(mDocumentRoot, "..", "Library"));
    private string library_root = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.LibraryDirectory,
                                                                       NSSearchPathDomain.User)[0].Path;


    private MKMapView _map;
    private MKMapViewDelegate _mapDelegate;

    MKPlacemark orignPlaceMark;
    MKPlacemark destPlaceMark;

    double InitialZoomDelta = 0.1;

    MKCoordinateSpan InitialZoomSpan = new MKCoordinateSpan(0.1, 0.1);

    public MyViewController() {
    }

    public override void ViewDidLoad() {

      base.ViewDidLoad();


      Console.WriteLine($"documents : {document_root}");
      Console.WriteLine($"library   : {library_root}");
      Console.WriteLine("");
      Console.WriteLine("");

      NavigationItem.Title = "MapKit Sample";

      //Init Map
      _map = new MKMapView {
        MapType = MKMapType.Standard,
        ShowsUserLocation = true,
        ZoomEnabled = true,
        ScrollEnabled = true,
        ShowsBuildings = true,
        PitchEnabled = true,

      };

      this.SetToolbarItems(new UIBarButtonItem[] {

        new UIBarButtonItem(UIBarButtonSystemItem.Camera, (s,e) => {

          using (var snapShotOptions = new MKMapSnapshotOptions())
          {
            snapShotOptions.Region = _map.Region;
            snapShotOptions.Scale = UIScreen.MainScreen.Scale;
            snapShotOptions.Size = _map.Frame.Size;

            using (var snapShot = new MKMapSnapshotter(snapShotOptions))
            {
              snapShot.Start((snapshot, error) =>
              {
                if (error == null)
                {
                  snapshot.Image.SaveToPhotosAlbum(
                    (uiimage, imgError) =>
                    {
                      if (imgError == null)
                      {
                        new UIAlertView("Image Saved", "Map View Image Saved!", null, "OK", null).Show();
                      }

                    });
                }
              });
            }
          }
        }),
        new UIBarButtonItem(UIBarButtonSystemItem.Organize, (sender, e) => {
          CreateRoute();
        }),
        new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace, null),
        new UIBarButtonItem(UIBarButtonSystemItem.Rewind, (sender, e) => {
          InitialZoomDelta += 0.1;
          InitialZoomSpan = new MKCoordinateSpan(InitialZoomDelta, InitialZoomDelta);
          var newRegion = new MKCoordinateRegion(orignPlaceMark.Coordinate, InitialZoomSpan);
          _map.SetRegion(newRegion, true);
        }),
        new UIBarButtonItem(UIBarButtonSystemItem.FastForward, (sender, e) => {
          InitialZoomDelta -= 0.1;
          InitialZoomSpan = new MKCoordinateSpan(InitialZoomDelta, InitialZoomDelta);
          var newRegion = new MKCoordinateRegion(orignPlaceMark.Coordinate, InitialZoomSpan);
          _map.SetRegion(newRegion, true);
        }),

      }, false);

      this.NavigationController.ToolbarHidden = false;

      //Create new MapDelegate Instance
      _mapDelegate = new MapDelegate();

      //Add delegate to map
      _map.Delegate = _mapDelegate;

      View = _map;

      //Create Directions
      CreateRoute();

      Task.Run(async () => {
        await Task.Delay(3000);

        GeocodeToConsoleAsync("6 Forest Knoll Castle Hill 2154");
        GeocodeToConsoleAsync("19A Cook Street Baulkham Hills 2153");
        GeocodeToConsoleAsync("10 Century Circuit Baulkham Hills 2153");
        GeocodeToConsoleAsync("212/10 Century Circuit Baulkham Hills 2153");
        GeocodeToConsoleAsync("6 Forest Knoll Castle Hill 2154");
        GeocodeToConsoleAsync("1 THE PLACE PENRITH 2750");
        GeocodeToConsoleAsync("103/5 CELEBRATION DRIVE BELLA VISTA 2153");
        GeocodeToConsoleAsync("123 BOB ROAD BOBS FARM 2316");
        GeocodeToConsoleAsync("123 Bob road BOBS FARM 2316");
        GeocodeToConsoleAsync("123 The Road PENRITH 2750");
        GeocodeToConsoleAsync("16A HILL ST BLAYNEY 2799");
        GeocodeToConsoleAsync("3 Packard Ave CASTLE HILL 2154");
        GeocodeToConsoleAsync("42 harris street BECKENHAM 6107");
      });

    }

    private void CreateRoute() {
      //Create Origin and Dest Place Marks and Map Items to use for directions

      //Start at Vantage Office Norwest
      orignPlaceMark = new MKPlacemark(new CLLocationCoordinate2D(-33.732711, 150.9618983));
      var sourceItem = new MKMapItem(orignPlaceMark);

      //End at Xamarin Cambridge Office
      //6 Forest Knoll
      destPlaceMark = new MKPlacemark(new CLLocationCoordinate2D(-33.7359329, 151.0179703));

      //Sydney Opera House
      destPlaceMark = new MKPlacemark(new CLLocationCoordinate2D(-33.8567844, 151.2131027));

      //Sydney International Airport
      destPlaceMark = new MKPlacemark(new CLLocationCoordinate2D(-33.9353852, 151.1633858));


      destPlaceMark = new MKPlacemark(new CLLocationCoordinate2D(-33.9604298, 151.1425861));


      var destItem = new MKMapItem(destPlaceMark);

      //Create Directions request using the source and dest items
      var request = new MKDirectionsRequest {
        Source = sourceItem,
        Destination = destItem,
        RequestsAlternateRoutes = true
      };

      var directions = new MKDirections(request);

      //Hit Apple Directions server
      directions.CalculateDirections((response, error) => {
        if (error != null) {
          Console.WriteLine(error.LocalizedDescription);
        } else {

          var newRegion = new MKCoordinateRegion(orignPlaceMark.Coordinate, InitialZoomSpan);
          _map.SetRegion(newRegion, true);

          Console.WriteLine($"_________________________________________________________________________________________");
          Console.WriteLine($"We found {response.Routes.Length} routes:");
          var i = 1;
          foreach (var route in response.Routes) {
            Console.WriteLine($"   {i}) {route.Name}  {route.Distance}m  {route.ExpectedTravelTime}seconds");
            i++;
          }

          //Add each polyline from route to map as overlay
          foreach (var route in response.Routes) {
            _map.AddOverlay(route.Polyline);

            Console.WriteLine($"_________________________________________________________________________________________");
            Console.WriteLine($"ROUTE INSTRUCTIONS:  {route.Name}   {route.Distance}m  {route.ExpectedTravelTime}seconds");

            if ((route.AdvisoryNotices != null) && (route.AdvisoryNotices.Length > 0)) {
              Console.WriteLine($"                     Route Notices:");
              foreach (var notice in route.AdvisoryNotices) {
                Console.WriteLine($"                         {notice}");
              }
            }

            Console.WriteLine($"_________________________________________________________________________________________");
            foreach (var step in route.Steps) {
              Console.WriteLine($"    {step.Distance}  {step.Instructions}     : {step.Polyline.Coordinate.ToString()}");
              if (step.Notice != null) {
                Console.WriteLine($"                     Notice: {step.Notice} ");
              }
            }
            Console.WriteLine($"_________________________________________________________________________________________");
          }
        }
      });
    }

    class MapDelegate : MKMapViewDelegate {
      //Override OverLayRenderer to draw Polyline returned from directions
      public override MKOverlayRenderer OverlayRenderer(MKMapView mapView, IMKOverlay overlay) {
        if (overlay is MKPolyline) {
          var route = (MKPolyline)overlay;
          var renderer = new MKPolylineRenderer(route) { StrokeColor = UIColor.Blue };
          return renderer;
        }
        return null;
      }
    }

    async void GeocodeToConsoleAsync(string address) {

      try {
        var geoCoder = new CLGeocoder();
        var placemarks = await geoCoder.GeocodeAddressAsync(address);
        foreach (var placemark in placemarks) {
          Console.WriteLine($"{address} : {placemark.Location.Coordinate.ToString()}");
        }
      } catch (Exception ex) {
        Console.WriteLine($"{address} : Not found");
      }
    }

  }
}