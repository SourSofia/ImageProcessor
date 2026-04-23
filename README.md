# 📌 Image Processor (C# WinForms)

## 📖 Description
This project is a Windows Forms-based image processing application developed using C#. It allows users to apply various image filters and transformations, including real-time webcam processing and background subtraction.  

The goal of this project is to demonstrate understanding of image manipulation, pixel-level processing, and basic computer vision concepts.

---

## ⚙️ Features
- Load and display images from local files  
- Copy and display images  
- Grayscale, Sepia, and Color Inversion filters  
- Histogram generation  
- Multiple convolution filters (Blur, Sharpen, Emboss, etc.)  
- Real-time webcam processing  
- Background subtraction (green screen effect)  
- Save processed images  

---

## 🛠️ Technologies Used
- C# (.NET Windows Forms)  
- AForge.NET (Webcam capture)  
- System.Drawing (image processing)  

---

## 🚀 How to Run
1. Open the project in Visual Studio  
2. Install required dependencies:
   - AForge.Video  
   - AForge.Video.DirectShow  
   - WebCamLib  
3. Build and run the project  
4. Use the menu/buttons to load images or start webcam processing  

---

## 🧠 What I Learned
Through this project, I learned how image processing works at the pixel level, including filtering, convolution, and color manipulation. I also gained experience in handling real-time video input and optimizing performance for image-heavy operations.

---

## 💡 Notes
- Webcam must be connected for live processing features  
- Some filters require a background image setup (for subtraction feature)  
