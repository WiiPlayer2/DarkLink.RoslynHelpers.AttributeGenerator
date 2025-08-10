{
  description = "A very basic flake";

  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs?ref=nixos-unstable";
  };

  outputs = { self, nixpkgs }: {
    apps.x86_64-linux.ci_publish =
      let
        pkgs = nixpkgs.legacyPackages.x86_64-linux;
        script = pkgs.writeShellApplication {
          name = "ci_publish";
          runtimeInputs = with pkgs; [
            dotnet-sdk
          ];
          text = ''
            dotnet build --configuration Release
            dotnet test --configuration Release
            dotnet pack --configuration Release --output ./packages \
              -p:RealPackageId=DarkLink.RoslynHelpers.AttributeGenerator
            dotnet pack ./DarkLink.RoslynHelpers.AttributeGenerator/DarkLink.RoslynHelpers.AttributeGenerator.csproj \
              --configuration Release --output ./packages \
              -p:RealPackageId=DarkLink.RoslynHelpers.AttributeGenerator.Bootstrap
            dotnet nuget push ./packages/* --skip-duplicate --source "$NUGET_REPO" --api-key "$NUGET_APIKEY"
          '';
        };
      in
      {
        type = "app";
        program = pkgs.lib.getExe script;
      };
  };
}
