# Matrix
---

## Overview
Matrix is a sanctuary for Web3 pioneers and deep-tech enthusiasts, designed to filter out the noise of mainstream social media. We provide a pure, focused environment for high-quality discourse. Here, an on-chain credential is the only pass, ensuring a community built on expertise and shared consensus. While the platform may start with fragmented sparks of insight, we believe that as true peers connect, these unordered glimmers will form a grand and orderly matrix of thought.

## Tools
- ASP\.NET Core 8 Web MVC
- Tailwindcss
- DaisyUI

## Steps
First, you need to install libman if you don't have or doesn't use Visual Studio, please enter this command line to your terminal:
```
dotnet tool install Microsoft.Web.LibraryManager.Cli
```
After installed libman, you need to install the dependency packages into the `wwwroot/lib` folder.
```
dotnet tool run libman restore
```
Then, you also need to install DotNetEnv so that the project can automatically connect to the database using the `.env` file.
<i><b>If you are using Visual Studio, you can install it in Nuget Extensions Management.</b></i>
```
dotnet add package DotNetEnv
```
Because this project used DaisyUI UI Library, you need to install tailwindcss CLI and DaisyUI.<br>

  1. Get Tailwind CSS executable
  FollowTailwind CSS guideand get the latest version of Tailwind CSS executable for your OS.

      ###### windows
      ```
      curl -sLo tailwindcss.exe https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-windows-x64.exe
      ```
      ###### MacOS
      ```
      curl -sLo tailwindcss https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-macos-arm64
      curl -sLo tailwindcss https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-macos-x64
      ```
      Make the file executable (For Linux and MacOS):
      ```
      chmod +x tailwindcss
      ```

  2. Get daisyUI bundled JS file (already have)
  3. Build CSS
      When you execute the following command, "tailwindcss" will be listened in the background.
      ###### MacOS
      ```
      npm run dev:mac
      ```
      ###### windows
      ```
      npm run dev:win
      ```
