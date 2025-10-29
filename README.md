# Windows Communication Foundation (WCF) and Web Service (WS) test client application
[![Auto build](https://github.com/DKorablin/Plugin.WcfClient/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/Plugin.WcfClient/releases/latest)

This application is a versatile test client for both WCF (Windows Communication Foundation) and classic ASMX Web Services. It allows you to add services via URL or local WSDL files, create test data for methods, and save/load entire test projects for later use.

Inspired by the original Microsoft WCF Test Client, this tool expands on its capabilities by adding full support for legacy Web Services (ASMX), making it a single solution for testing various service-oriented architectures.

## Key Improvements

1. __Manual Client Regeneration__: You can manually regenerate the client proxy libraries at any time. This is invaluable for regression testing, allowing you to quickly check if recent service updates have introduced any breaking changes.

2. __Integrated Test Data__: All test data for service methods is saved directly within your test project. This eliminates the need to store test values in external files or documents, streamlining your workflow.

3. __Saved Endpoints__: The application saves your service endpoints, so you can easily switch between and test different service environments (like development, staging, or production) without having to re-enter the addresses each time.