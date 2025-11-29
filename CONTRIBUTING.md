# Contributing to xZenith Hardware Monitor

Thank you for your interest in contributing to xZenith Hardware Monitor! This document provides guidelines and information for contributors.

## Project Structure

The project consists of two main components:

- **xZenithHardwareMonitorLib** - Core hardware monitoring library (C#/.NET)
- **xZenithHardwareMonitorAPI** - C-compatible API wrapper (C++/CLI)

## Getting Started

### Prerequisites

- Windows 10/11
- Visual Studio 2017 or later
- .NET Framework 4.7.2 SDK
- Windows SDK
- C++/CLI support (install via Visual Studio Installer)

### Building the Project

1. Clone the repository
2. Build xZenithHardwareMonitorLib first:
   - Open `xZenithHardwareMonitorLib/xZenithHardwareMonitor.sln`
   - Build in Release/Any CPU configuration
3. Build xZenithHardwareMonitorAPI:
   - Open `xZenithHardwareMonitorAPI/xZenithHardwareMonitorAPI.sln`
   - Build in Release/x64 configuration

## How to Contribute

### Reporting Issues

- Use GitHub Issues to report bugs
- Include system information (OS version, hardware)
- Provide steps to reproduce the issue
- Include any error messages or logs

### Submitting Changes

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/your-feature`)
3. Make your changes
4. Test thoroughly
5. Commit with clear messages
6. Push to your fork
7. Open a Pull Request

### Code Style Guidelines

#### C# (xZenithHardwareMonitorLib)

- Follow Microsoft's C# coding conventions
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Keep methods focused and small

#### C++/CLI (xZenithHardwareMonitorAPI)

- Follow consistent naming conventions
- Document exported functions
- Handle memory management carefully
- Ensure proper cleanup in destructors

### Testing

Before submitting:

1. Build both projects successfully
2. Test with your target application (Tauri, Rust, etc.)
3. Verify JSON output is valid
4. Test on different hardware configurations if possible
5. Ensure administrator privileges work correctly

## Areas for Contribution

### High Priority

- Additional hardware support
- Bug fixes for specific motherboard models
- Performance optimizations
- Better error handling

### Documentation

- API usage examples
- Integration guides for different languages
- Hardware compatibility notes

### Features

- Additional sensor types
- Configuration options
- Caching mechanisms
- Event-based notifications

## Code of Conduct

- Be respectful and inclusive
- Provide constructive feedback
- Help others learn and grow
- Focus on the technical merits

## Questions?

If you have questions about contributing, feel free to open an issue with the "question" label.

## License

By contributing, you agree that your contributions will be licensed under the same license as the component you're contributing to:

- xZenithHardwareMonitorAPI: MIT License
- xZenithHardwareMonitorLib: MPL-2.0
