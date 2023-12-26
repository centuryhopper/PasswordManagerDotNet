// using System.Text;
// using Microsoft.AspNetCore.Components;
// using Microsoft.AspNetCore.Components.Authorization;
// using Microsoft.JSInterop;
// using Radzen;
// using Radzen.Blazor;

// namespace BlazorServerPasswordManager.Components.Pages;

// public class PasswordTableBase : ComponentBase
// {
    

//     protected RadzenUpload upload;

//     protected void OnChange(UploadChangeEventArgs args, string name)
//     {
//         foreach (var file in args.Files)
//         {
//             Console.WriteLine($"File: {file.Name} / {file.Size} bytes");
//         }

//         Console.WriteLine($"{name} changed");
//     }
//     protected void OnProgress(UploadProgressArgs args, string name)
//     {
//         Console.WriteLine($"{args.Progress}% '{name}' / {args.Loaded} of {args.Total} bytes.");

//         if (args.Progress == 100)
//         {
//             foreach (var file in args.Files)
//             {
//                 Console.WriteLine($"Uploaded: {file.Name} / {file.Size} bytes");
//             }
//         }
//     }

// }

