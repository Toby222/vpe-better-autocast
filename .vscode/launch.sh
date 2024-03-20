set -e

script_dir=$(dirname $(readlink -f $0))
mod_dir=$(dirname $script_dir)
pushd $script_dir

LC_ALL=C steam-run ~/.steam/steam/steamapps/common/RimWorld/RimWorldLinux -logfile $mod_dir/log/rimworld.log -popupwindow $@

popd