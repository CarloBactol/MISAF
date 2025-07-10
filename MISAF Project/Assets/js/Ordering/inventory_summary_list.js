/*const table = new DataTable('#inventory_summary_list-datatable');*/

//search: { return: true },
//select: { style: "multi" },
$(document).ready(function () {
    "use strict";
    recall_datatable();
});

function recall_datatable() {
    $("#inventory_summary_list-datatable").DataTable({
        language: {
            paginate: {
                previous: "<i class='mdi mdi-chevron-left'>",
                next: "<i class='mdi mdi-chevron-right'>"
            },
            info: "Showing Inventory _START_ to _END_ of _TOTAL_",
            lengthMenu: 'Display <select class=\'form-select form-select-sm ms-1 me-1\'><option value="5">5</option><option value="10">10</option><option value="20">20</option><option value="-1">All</option></select> Inventory'
        },
        pageLength: 10,
        stateSave: true,
        order: [[0, "asc"]],
        buttons: ['copy', 'excel', 'pdf'],
        createdRow: (row, data, index) => {
            for (var i = 1; i < 14; i++) {
                var _val = data[5].replace(/[\$,]/g, '') * 1;
                if (_val == 0) { data[i] = "-"; } else if (_val > 0) { data[i] = numberFormatter.format(_val); }
            }
        },
        drawCallback: function () {
            $(".dataTables_paginate > .pagination").addClass("pagination-rounded"), $("#inventory_summary_list-datatable_length label").addClass("form-label"), document.querySelector(".dataTables_wrapper .row").querySelectorAll(".col-md-6").forEach(function (e) {
                e.classList.add("col-sm-6"), e.classList.remove("col-sm-12"), e.classList.remove("col-md-6")
            });

            $("tr > td > label").each(function () {
                var $control = $(this);
                var $value = $control.html();
                var floatValue = parseFloat($value);

                if (floatValue === undefined) {
                } else {
                    if (floatValue == 0) {
                        $control.html('-');
                    } else if (floatValue > 0) {
/*                        $control.html(numberFormatter.format(floatValue));*/
                    }
                }
            });
        }
    });
}

//table.on('click', 'tbody tr', (e) => {
//    let classList = e.currentTarget.classList;
//    if (classList.contains('selected')) {
//        classList.remove('selected');
//    }
//    else {
//        table.rows('.selected').nodes().each((row) => row.classList.remove('selected'));
//        classList.add('selected');
//    }
//});

//document.querySelector('#button').addEventListener('click', function () {
//    table.row('.selected').remove().draw(false);
//});