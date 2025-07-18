/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./src/**/*.{html,ts}"],
  theme: {
    extend: {
      colors: {
        light: "#F9FAFB", // A very light gray, almost white
        dark: "#1F2937", // A dark gray for text and backgrounds
        primary: "#4F46E5", // Indigo 600
        secondary: "#6B7280", // Gray 500
        success: "#28a745",
        danger: "#dc3545",
        warning: "#ffc107",
        info: "#17a2b8",
        // Adding more shades for primary and secondary to allow for more nuanced designs
        "primary-light": "#818CF8", // Indigo 400
        "primary-dark": "#3730A3", // Indigo 800
        "secondary-light": "#D1D5DB", // Gray 300
        "secondary-dark": "#4B5563", // Gray 700
        "success-light": "#D4EDDA", // A lighter shade for success backgrounds
        "success-dark": "#1E7E34", // A darker shade for success text/borders
        "danger-light": "#F8D7DA", // A lighter shade for danger backgrounds
        "danger-dark": "#A71C2B", // A darker shade for danger text/borders
      },
    },
  },
  plugins: [],
};
