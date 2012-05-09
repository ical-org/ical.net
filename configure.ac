dnl Warning: This is an automatically generated file, do not edit!
dnl Process this file with autoconf to produce a configure script.
AC_PREREQ([2.54])
AC_INIT([DDay.iCal], [0.1])
AM_INIT_AUTOMAKE([foreign])
AM_MAINTAINER_MODE

dnl pkg-config
AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno"; then
        AC_MSG_ERROR([You need to install pkg-config])
fi

SHAMROCK_EXPAND_LIBDIR
SHAMROCK_EXPAND_BINDIR
SHAMROCK_EXPAND_DATADIR

AC_PROG_INSTALL

AC_PATH_PROG(GMCS, gmcs, no)
if test "x$GMCS" = "xno"; then
        AC_MSG_ERROR([gmcs Not found])
fi


AC_ARG_ENABLE(debug,
	AC_HELP_STRING([--enable-debug],
		[Use 'DEBUG' Configuration [default=YES]]),
		enable_debug=yes, enable_debug=no)
AM_CONDITIONAL(ENABLE_DEBUG, test x$enable_debug = xyes)
if test "x$enable_debug" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
AC_ARG_ENABLE(debug_mixed_platforms,
	AC_HELP_STRING([--enable-debug_mixed_platforms],
		[Use 'DEBUG_MIXED_PLATFORMS' Configuration [default=NO]]),
		enable_debug_mixed_platforms=yes, enable_debug_mixed_platforms=no)
AM_CONDITIONAL(ENABLE_DEBUG_MIXED_PLATFORMS, test x$enable_debug_mixed_platforms = xyes)
if test "x$enable_debug_mixed_platforms" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
AC_ARG_ENABLE(debug_x86,
	AC_HELP_STRING([--enable-debug_x86],
		[Use 'DEBUG_X86' Configuration [default=NO]]),
		enable_debug_x86=yes, enable_debug_x86=no)
AM_CONDITIONAL(ENABLE_DEBUG_X86, test x$enable_debug_x86 = xyes)
if test "x$enable_debug_x86" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
AC_ARG_ENABLE(release,
	AC_HELP_STRING([--enable-release],
		[Use 'RELEASE' Configuration [default=NO]]),
		enable_release=yes, enable_release=no)
AM_CONDITIONAL(ENABLE_RELEASE, test x$enable_release = xyes)
if test "x$enable_release" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
AC_ARG_ENABLE(release_mixed_platforms,
	AC_HELP_STRING([--enable-release_mixed_platforms],
		[Use 'RELEASE_MIXED_PLATFORMS' Configuration [default=NO]]),
		enable_release_mixed_platforms=yes, enable_release_mixed_platforms=no)
AM_CONDITIONAL(ENABLE_RELEASE_MIXED_PLATFORMS, test x$enable_release_mixed_platforms = xyes)
if test "x$enable_release_mixed_platforms" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
AC_ARG_ENABLE(release_x86,
	AC_HELP_STRING([--enable-release_x86],
		[Use 'RELEASE_X86' Configuration [default=NO]]),
		enable_release_x86=yes, enable_release_x86=no)
AM_CONDITIONAL(ENABLE_RELEASE_X86, test x$enable_release_x86 = xyes)
if test "x$enable_release_x86" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
if test -z "$CONFIG_REQUESTED" ; then
	AM_CONDITIONAL(ENABLE_DEBUG, true)
	enable_debug=yes
fi


dnl package checks, common for all configs

dnl package checks, per config


AC_CONFIG_FILES([
DDay.Collections/DDay.Collections/dday.collections.pc
DDay.Collections/DDay.Collections/Makefile
DDay.iCal/dday.ical.pc
DDay.iCal/Makefile
DDay.iCal.Test/dday.ical.test
DDay.iCal.Test/Makefile
Makefile

])

AC_OUTPUT
