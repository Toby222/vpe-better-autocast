{
  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
  };

  outputs =
    {
      self,
      nixpkgs,
    }:
    let
      supportedSystems = [ "x86_64-linux" ];
      forEachSupportedSystem =
        f: nixpkgs.lib.genAttrs supportedSystems (system: f { pkgs = import nixpkgs { inherit system; }; });
    in
    {
      devShells = forEachSupportedSystem (
        { pkgs, ... }: {
          default = pkgs.mkShell {
            packages =
              with pkgs;
              let
                sdk =
                  with dotnetCorePackages;
                  combinePackages [
                    sdk_11_0
                    sdk_10_0
                  ];
              in
              [
                sdk
                nixfmt
                libxslt.bin
                nodejs
              ];
          };
        }
      );
    };
}
