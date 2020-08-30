let 
  sources = import ./nix/sources.nix;
  pkgs = import sources.nixpkgs {};
in
pkgs.mkShell {
  buildInputs = with pkgs; [
    dotnet-sdk_3
    nodePackages.graphql-cli
    # keep this line if you use bash
    pkgs.bashInteractive
  ];
}
