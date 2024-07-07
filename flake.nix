{
  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs/nixos-unstable";
    flake-parts.url = "github:hercules-ci/flake-parts";
    systems.url = "github:nix-systems/default";
    # Dev tools
    treefmt-nix = {
      url = "github:numtide/treefmt-nix";
      inputs.nixpkgs.follows = "nixpkgs";
    };
  };

  outputs =
    inputs:
    inputs.flake-parts.lib.mkFlake { inherit inputs; } {
      systems = import inputs.systems;
      imports = [ inputs.treefmt-nix.flakeModule ];
      perSystem =
        {
          config,
          self',
          pkgs,
          lib,
          system,
          ...
        }:
        {
          devShells.default = pkgs.mkShell {
            inputsFrom = [ config.treefmt.build.devShell ];
            nativeBuildInputs = with pkgs; [
              dotnetCorePackages.dotnet_8.sdk
              dotnetCorePackages.dotnet_8.runtime
              nixfmt-rfc-style
              omnisharp-roslyn
              mono
              libxslt
              nodePackages.npm
            ];

            DOTNET_ROOT = "${pkgs.dotnet-sdk_8}";
          };

          # Add your auto-formatters here.
          # cf. https://numtide.github.io/treefmt/
          treefmt.config = {
            projectRootFile = "flake.nix";
            programs = {
              csharpier.enable = true;
              nixfmt.enable = true;
              shfmt.enable = true;
              prettier = {
                enable = true;
                includes = [
                  "*.xml"
                  "*.xslt"
                  "*.json"
                  "*.csproj"
                ];
                settings = {
                  plugins = [ "@prettier/plugin-xml" ];
                  bracketSameLine = true;
                };
              };
            };
            settings.global.excludes = [
              "*.ase"
              "*.dll"
              "*.png"
            ];
          };
        };
    };
}
